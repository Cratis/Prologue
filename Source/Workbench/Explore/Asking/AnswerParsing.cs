// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Asking;

/// <summary>
/// Reads an agent's reply into an answer and the command shapes it cites.
/// </summary>
public static class AnswerParsing
{
    /// <summary>
    /// Parses a reply of the shape {"answer": "...", "citations": ["..."]}, which may be wrapped in a code fence or in words. A
    /// citation that does not name a captured command shape is dropped, never invented into one.
    /// </summary>
    /// <param name="reply">The text the agent replied with.</param>
    /// <param name="known">The command shapes that were captured.</param>
    /// <returns>The answer, or null when the reply holds no JSON object with an answer.</returns>
    public static ParsedAnswer? Parse(string reply, IEnumerable<CommandShape> known)
    {
        var start = reply.IndexOf('{');
        var end = reply.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(reply[start..(end + 1)]);
            var text = Property(document.RootElement, "answer");
            if (text.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(text.GetString()))
            {
                return null;
            }

            var cited = Property(document.RootElement, "citations");
            var citations = cited.ValueKind == JsonValueKind.Array
                ? cited.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString() ?? string.Empty)
                : [];
            return new(text.GetString()!.Trim(), Existing(citations, known));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    static JsonElement Property(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object
            ? element.EnumerateObject().FirstOrDefault(property => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)).Value
            : default;

    static IEnumerable<CommandShape> Existing(IEnumerable<string> citations, IEnumerable<CommandShape> known)
    {
        var shapes = known.ToList();
        return [.. citations
            .Select(citation => shapes.Find(shape => string.Equals(Normalize(shape.Value), Normalize(citation), StringComparison.OrdinalIgnoreCase)))
            .OfType<CommandShape>()
            .Distinct()];
    }

    static string Normalize(string shape) => string.Join(' ', shape.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
