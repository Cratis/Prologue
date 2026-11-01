// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// Merges a new interpretation into the suggestions people have been reviewing.
/// </summary>
public static class ReviewMerge
{
    /// <summary>
    /// Merges what an interpretation found into what is already reviewed. A suggestion people have not seen is added as suggested. One they
    /// know takes the new evidence and keeps its status and a name a person chose, and is no longer withdrawn. One the interpretation no
    /// longer finds is withdrawn, never deleted, so a decision made on it is not lost.
    /// </summary>
    /// <param name="reviewed">The suggestions as they are reviewed.</param>
    /// <param name="found">The suggestions the interpretation found.</param>
    /// <returns>The suggestions after the merge, in the order of the interpretation followed by the withdrawn ones.</returns>
    public static IEnumerable<ReviewedSuggestion> Merge(IEnumerable<ReviewedSuggestion> reviewed, IEnumerable<SuggestedEvent> found)
    {
        var known = reviewed.ToDictionary(suggestion => suggestion.Suggestion.Key);
        var merged = found.Select(suggestion => known.TryGetValue(suggestion.Key, out var existing)
            ? existing with { Suggestion = suggestion, Name = existing.Renamed ? existing.Name : suggestion.Refined, Withdrawn = false }
            : new ReviewedSuggestion(suggestion, SuggestionStatus.Suggested, suggestion.Refined, false, false)).ToList();
        var keys = merged.Select(suggestion => suggestion.Suggestion.Key).ToHashSet();
        return [.. merged, .. reviewed.Where(suggestion => !keys.Contains(suggestion.Suggestion.Key)).Select(suggestion => suggestion with { Withdrawn = true })];
    }
}
