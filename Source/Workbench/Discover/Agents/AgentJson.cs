// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Prologue.Workbench.Discover.Agents;

/// <summary>
/// Finds the JSON in an agent's answer, which may be wrapped in a code fence or surrounded by prose.
/// </summary>
public static class AgentJson
{
    static readonly JsonDocumentOptions _options = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };

    /// <summary>
    /// Reads the entries under a property of the answer. An answer that is a bare array is taken as the entries.
    /// </summary>
    /// <param name="text">The answer.</param>
    /// <param name="property">The property that holds the entries.</param>
    /// <param name="problem">Why nothing could be read, when nothing could.</param>
    /// <returns>The entries; empty when the answer is not usable.</returns>
    public static IReadOnlyList<JsonElement> Entries(string text, string property, out string? problem)
    {
        problem = null;
        var json = Extract(text);
        if (json is null)
        {
            problem = "The answer holds no JSON";
            return [];
        }

        try
        {
            var root = JsonElement.Parse(json, _options);
            if (root.ValueKind == JsonValueKind.Array)
            {
                return [.. root.EnumerateArray()];
            }

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(property, out var entries) && entries.ValueKind == JsonValueKind.Array)
            {
                return [.. entries.EnumerateArray()];
            }

            problem = $"The answer has no '{property}' list";
            return [];
        }
        catch (JsonException exception)
        {
            problem = $"The answer is not valid JSON: {exception.Message}";
            return [];
        }
    }

    /// <summary>
    /// Reads a string property, trimmed.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="name">The property.</param>
    /// <returns>The text; empty when the property is missing or not text.</returns>
    public static string Text(JsonElement entry, string name) =>
        entry.ValueKind == JsonValueKind.Object && entry.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()!.Trim() : string.Empty;

    /// <summary>
    /// Reads an integer property, which an agent may write as a number or as text.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="name">The property.</param>
    /// <returns>The number; zero when the property is missing or not a number.</returns>
    public static int Number(JsonElement entry, string name)
    {
        if (entry.ValueKind != JsonValueKind.Object || !entry.TryGetProperty(name, out var value))
        {
            return 0;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        return value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed) ? parsed : 0;
    }

    /// <summary>
    /// Reads a list of strings.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="name">The property.</param>
    /// <returns>The non-blank strings; empty when the property is missing.</returns>
    public static IReadOnlyList<string> Texts(JsonElement entry, string name) =>
        entry.ValueKind == JsonValueKind.Object && entry.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array
            ? [.. value.EnumerateArray().Where(_ => _.ValueKind == JsonValueKind.String).Select(_ => _.GetString()!.Trim()).Where(_ => _.Length > 0)]
            : [];

    static string? Extract(string text)
    {
        var fenced = text.IndexOf("```", StringComparison.Ordinal);
        var body = text;
        if (fenced >= 0)
        {
            var start = text.IndexOf('\n', fenced);
            var end = start < 0 ? -1 : text.IndexOf("```", start, StringComparison.Ordinal);
            if (start >= 0 && end > start)
            {
                body = text[(start + 1)..end];
            }
        }

        var open = body.IndexOfAny(['{', '[']);
        if (open < 0)
        {
            return null;
        }

        var close = body[open] == '{' ? body.LastIndexOf('}') : body.LastIndexOf(']');
        return close > open ? body[open..(close + 1)] : null;
    }
}
