// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Cratis.Prologue.Workbench.Events.Reviewing;

/// <summary>
/// What makes a good name for an event. A name that is not PascalCase is refused; one that does not read as a past-tense fact only
/// gets advice, because the language has more irregular verbs than a rule can know.
/// </summary>
public static partial class EventNames
{
    static readonly HashSet<string> _irregularPastTense =
    [
        with(StringComparer.OrdinalIgnoreCase),
        "Sent", "Paid", "Built", "Made", "Lent", "Left", "Kept", "Sold", "Bought", "Taken", "Given", "Written", "Done", "Hidden",
        "Began", "Won", "Lost", "Found", "Held", "Set", "Put", "Read", "Cut", "Shut", "Spent", "Brought", "Became", "Chosen", "Drawn",
        "Gone", "Known", "Seen", "Shown", "Told", "Thrown", "Worn", "Withdrawn", "Overdue"
    ];

    [GeneratedRegex("^[A-Z][A-Za-z0-9]*$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PascalCase { get; }

    [GeneratedRegex("[A-Z][a-z0-9]*|[a-z0-9]+", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Words { get; }

    /// <summary>
    /// Gets whether a name is PascalCase: a capital first, then letters and digits only.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>True when it is.</returns>
    public static bool IsPascalCase(string name) => PascalCase.IsMatch(name);

    /// <summary>
    /// Gets the advice for a name, or nothing when it reads as a past-tense fact.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The advice; empty when there is none.</returns>
    public static string Advice(string name)
    {
        var last = LastWord(name);
        var reads = last.EndsWith("ed", StringComparison.Ordinal) || _irregularPastTense.Contains(last);
        return reads || name.Length == 0 ? string.Empty : "Past-tense names read best as facts, for example BookLent or AuthorRegistered.";
    }

    static string LastWord(string name)
    {
        var words = Words.Matches(name);
        return words.Count == 0 ? name : words[^1].Value;
    }
}
