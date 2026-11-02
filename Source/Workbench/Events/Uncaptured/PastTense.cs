// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// Puts a verb in the past tense, for the mechanical names of events nobody captured. The names are provisional: a person or the Interpreter agent refines them.
/// </summary>
public static class PastTense
{
    static readonly Dictionary<string, string> _irregular = new(StringComparer.OrdinalIgnoreCase)
    {
        ["lend"] = "Lent",
        ["pay"] = "Paid",
        ["send"] = "Sent",
        ["make"] = "Made",
        ["set"] = "Set",
        ["put"] = "Put",
        ["hold"] = "Held",
        ["buy"] = "Bought",
        ["sell"] = "Sold",
        ["pick"] = "Picked"
    };

    static readonly HashSet<string> _doubling = new(StringComparer.OrdinalIgnoreCase) { "ship", "submit", "stop", "drop", "plan", "transfer", "admit", "commit", "refer", "permit" };

    /// <summary>
    /// Puts a verb in the past tense.
    /// </summary>
    /// <param name="verb">The verb, in lower or upper case.</param>
    /// <returns>The past tense in PascalCase, for example Returned or Lent.</returns>
    public static string Of(string verb)
    {
        var word = verb.Trim().ToLowerInvariant();
        if (word.Length == 0)
        {
            return string.Empty;
        }

        if (_irregular.TryGetValue(word, out var known))
        {
            return known;
        }

        var past = word switch
        {
            _ when word.EndsWith("ed", StringComparison.Ordinal) => word,
            _ when word.EndsWith('e') => $"{word}d",
            _ when word.EndsWith('y') && word.Length > 1 && !"aeiou".Contains(word[^2]) => $"{word[..^1]}ied",
            _ when _doubling.Contains(word) => $"{word}{word[^1]}ed",
            _ => $"{word}ed"
        };
        return char.ToUpperInvariant(past[0]) + past[1..];
    }
}
