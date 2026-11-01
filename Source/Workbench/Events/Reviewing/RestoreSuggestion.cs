// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Reviewing;

/// <summary>
/// Restores a rejected suggestion to the ones still to review. Restoring what is not rejected records nothing.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Key">The suggestion.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record RestoreSuggestion(PrologueId Prologue, SuggestionKey Key)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="review">The review, read under the command's consistency boundary.</param>
    /// <returns>The event, nothing, or a refusal.</returns>
    public Result<IEnumerable<EventSuggestionRestored>, ValidationResult> Handle(EventReview? review)
    {
        if (ReviewRules.Find(review, Key, out var suggestion) is { } missing)
        {
            return missing;
        }

        IEnumerable<EventSuggestionRestored> events = suggestion.Status == SuggestionStatus.Rejected ? [new EventSuggestionRestored(Key)] : [];
        return Result<IEnumerable<EventSuggestionRestored>, ValidationResult>.Success(events);
    }
}

/// <summary>
/// A rejected suggestion was restored to the ones still to review.
/// </summary>
/// <param name="Key">The suggestion.</param>
[EventType]
public record EventSuggestionRestored(SuggestionKey Key);
