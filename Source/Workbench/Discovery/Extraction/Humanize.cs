// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Cratis.Prologue.Workbench.Discovery.Extraction;

/// <summary>
/// Turns code names into the words a person would use for them.
/// </summary>
public static partial class Humanize
{
    [GeneratedRegex(@"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|[_\-\s]+", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex WordBoundary { get; }

    /// <summary>
    /// Turns a method name such as "ReturnLoanAsync" into a capability name such as "Return loan".
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The words, with the first capitalized.</returns>
    public static string Name(string name)
    {
        var trimmed = name.EndsWith("Async", StringComparison.Ordinal) && name.Length > 5 ? name[..^5] : name;
        var words = WordBoundary.Split(trimmed).Where(word => word.Length > 0).ToList();
        if (words.Count == 0)
        {
            return name;
        }

        var text = string.Join(' ', words.Select((word, index) => index == 0 || IsAcronym(word) ? word : word.ToLowerInvariant()));
        return char.ToUpperInvariant(text[0]) + text[1..];
    }

    static bool IsAcronym(string word) => word.Length > 1 && word.All(char.IsUpper);
}
