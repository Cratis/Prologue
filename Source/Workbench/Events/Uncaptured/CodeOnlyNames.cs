// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Events.Interpreting;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// Derives the mechanical name of an event from the route the code serves, for a capability nobody captured: the entity in the route and what the route does to it.
/// </summary>
public static class CodeOnlyNames
{
    static readonly HashSet<string> _queries = [with(StringComparer.Ordinal), "GET", "HEAD", "OPTIONS"];

    static readonly string[] _ignored = ["api", "v1", "v2", "v3"];

    /// <summary>
    /// Derives the name.
    /// </summary>
    /// <param name="shape">The command shape, as METHOD /path with {id} for every identifying segment.</param>
    /// <returns>The past-tense event name; empty when the shape does not change anything (a query) or names nothing.</returns>
    public static string From(string shape)
    {
        var space = shape.IndexOf(' ');
        if (space < 0)
        {
            return string.Empty;
        }

        var method = shape[..space].ToUpperInvariant();
        if (_queries.Contains(method))
        {
            return string.Empty;
        }

        var tokens = shape[(space + 1)..].Split('/', StringSplitOptions.RemoveEmptyEntries).Where(token => !_ignored.Contains(token, StringComparer.OrdinalIgnoreCase)).ToList();
        var literals = tokens.Where(token => !token.StartsWith('{')).ToList();
        if (literals.Count == 0)
        {
            return string.Empty;
        }

        var last = tokens[^1];
        var isAction = !last.StartsWith('{') && literals.Count > 1 && !IsPlural(last);
        var resource = isAction ? literals[^2] : literals[^1];
        var entity = TableEvents.Entity(JoinWords(resource));
        if (!isAction)
        {
            return Naming.EventName(entity, OperationOf(method));
        }

        var words = Split(last);
        return $"{entity}{string.Concat(words.Skip(1).Select(Capitalize))}{PastTense.Of(words[0])}";
    }

    /// <summary>
    /// Derives the name for a capability that serves no route, such as a sweep the code runs on its own.
    /// </summary>
    /// <param name="capability">The capability, in words.</param>
    /// <returns>The event name; empty when the capability has no name.</returns>
    public static string FromCapability(string capability)
    {
        var name = string.Concat(Split(capability).Select(Capitalize));
        return name.Length == 0 ? string.Empty : $"{name}Completed";
    }

    static ChangeOperation OperationOf(string method) =>
        method switch
        {
            "DELETE" => ChangeOperation.Delete,
            "PUT" => ChangeOperation.Update,
            "PATCH" => ChangeOperation.Update,
            _ => ChangeOperation.Insert
        };

    static bool IsPlural(string token) => token.EndsWith('s') && !token.EndsWith("ss", StringComparison.OrdinalIgnoreCase);

    static string JoinWords(string token) => string.Concat(Split(token).Select(Capitalize));

    static string Capitalize(string word) => word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();

    static List<string> Split(string text) =>
        [.. JobMatching.Words(text).Split(' ', StringSplitOptions.RemoveEmptyEntries)];
}
