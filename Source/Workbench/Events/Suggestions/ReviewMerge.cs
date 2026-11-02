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
    /// <remarks>
    /// A suggestion that rests on code alone is identified by the event the code implies, which the captures may name differently once the
    /// command is captured. When a captured suggestion appears for a command a code-only suggestion was about, it takes over the decision made
    /// on the code-only one and the code-only one is dropped: the person decided about that command, and that is still what they decided about.
    /// </remarks>
    /// <param name="reviewed">The suggestions as they are reviewed.</param>
    /// <param name="found">The suggestions the interpretation found.</param>
    /// <returns>The suggestions after the merge, in the order of the interpretation followed by the withdrawn ones.</returns>
    public static IEnumerable<ReviewedSuggestion> Merge(IEnumerable<ReviewedSuggestion> reviewed, IEnumerable<SuggestedEvent> found)
    {
        var known = reviewed.ToDictionary(suggestion => suggestion.Suggestion.Key);
        var foundKeys = found.Select(suggestion => suggestion.Key).ToHashSet();
        var superseded = new HashSet<SuggestionKey>();
        var merged = found.Select(suggestion =>
        {
            if (known.TryGetValue(suggestion.Key, out var existing))
            {
                return existing with { Suggestion = suggestion, Name = existing.Renamed ? existing.Name : suggestion.Refined, Withdrawn = false };
            }

            var predecessor = suggestion.Origin == SuggestionOrigin.Captured ? Predecessor(reviewed, suggestion, foundKeys, superseded) : null;
            if (predecessor is null)
            {
                return new ReviewedSuggestion(suggestion, SuggestionStatus.Suggested, suggestion.Refined, false, false);
            }

            superseded.Add(predecessor.Suggestion.Key);
            return new ReviewedSuggestion(suggestion, predecessor.Status, predecessor.Renamed ? predecessor.Name : suggestion.Refined, predecessor.Renamed, false);
        }).ToList();
        return [.. merged, .. reviewed.Where(suggestion => !foundKeys.Contains(suggestion.Suggestion.Key) && !superseded.Contains(suggestion.Suggestion.Key)).Select(suggestion => suggestion with { Withdrawn = true })];
    }

    static ReviewedSuggestion? Predecessor(IEnumerable<ReviewedSuggestion> reviewed, SuggestedEvent captured, HashSet<SuggestionKey> foundKeys, HashSet<SuggestionKey> superseded) =>
        reviewed
            .Where(candidate => candidate.Suggestion.Origin == SuggestionOrigin.CodeOnly
                && !foundKeys.Contains(candidate.Suggestion.Key)
                && !superseded.Contains(candidate.Suggestion.Key)
                && candidate.Suggestion.Evidence.Shapes.Any(shape => captured.Evidence.Shapes.Any(seen => string.Equals(seen.Shape.Value, shape.Shape.Value, StringComparison.OrdinalIgnoreCase))))
            .OrderByDescending(candidate => candidate.Status != SuggestionStatus.Suggested || candidate.Renamed)
            .FirstOrDefault();
}
