// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Recording;

/// <summary>
/// Pairs what an analysis found with what is recorded, so a finding that was only worded differently is recognized as the one that is
/// already recorded instead of being recorded as a new fact next to a removed one.
/// </summary>
/// <typeparam name="TFound">The type of what was found.</typeparam>
/// <typeparam name="TRecorded">The type of what is recorded.</typeparam>
/// <param name="Pairs">The findings that are already recorded, each with what is recorded for it.</param>
/// <param name="New">The findings nothing recorded matches.</param>
/// <param name="Gone">What is recorded and nothing found matches.</param>
public record FindingPairs<TFound, TRecorded>(
    IReadOnlyList<(TFound Found, TRecorded Recorded)> Pairs,
    IReadOnlyList<TFound> New,
    IReadOnlyList<TRecorded> Gone);

/// <summary>
/// Builds <see cref="FindingPairs{TFound, TRecorded}"/>.
/// </summary>
public static class FindingPairs
{
    /// <summary>
    /// Pairs findings with what is recorded. The matchers are tried in order, strongest first: a finding is paired by the first
    /// matcher that finds a recorded item not paired yet, and each recorded item is paired at most once.
    /// </summary>
    /// <typeparam name="TFound">The type of what was found.</typeparam>
    /// <typeparam name="TRecorded">The type of what is recorded.</typeparam>
    /// <param name="found">What the analysis found.</param>
    /// <param name="recorded">What is recorded.</param>
    /// <param name="matchers">Tell whether a finding and a recorded item are the same fact, strongest first.</param>
    /// <returns>The pairs, the new findings and what is gone.</returns>
    public static FindingPairs<TFound, TRecorded> Of<TFound, TRecorded>(
        IReadOnlyList<TFound> found,
        IReadOnlyList<TRecorded> recorded,
        params Func<TFound, TRecorded, bool>[] matchers)
    {
        var pairs = new List<(TFound, TRecorded)>();
        var unmatchedFound = found.ToList();
        var unmatchedRecorded = recorded.ToList();
        foreach (var matches in matchers)
        {
            foreach (var item in unmatchedFound.ToList())
            {
                var index = unmatchedRecorded.FindIndex(candidate => matches(item, candidate));
                if (index < 0)
                {
                    continue;
                }

                pairs.Add((item, unmatchedRecorded[index]));
                unmatchedFound.Remove(item);
                unmatchedRecorded.RemoveAt(index);
            }
        }

        return new(pairs, unmatchedFound, unmatchedRecorded);
    }
}
