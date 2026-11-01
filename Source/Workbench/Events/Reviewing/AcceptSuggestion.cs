// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Reviewing;

/// <summary>
/// Accepts a suggested event into the model. A rejected suggestion cannot be accepted without restoring it first, and accepting what is accepted records nothing.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Key">The suggestion.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record AcceptSuggestion(PrologueId Prologue, SuggestionKey Key)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="review">The review, read under the command's consistency boundary.</param>
    /// <returns>The event, nothing, or a refusal.</returns>
    public Result<IEnumerable<EventSuggestionAccepted>, ValidationResult> Handle(EventReview? review)
    {
        if (ReviewRules.Find(review, Key, out var suggestion) is { } missing)
        {
            return missing;
        }

        if (suggestion.Status == SuggestionStatus.Rejected)
        {
            return ValidationResult.Error("This event was rejected. Restore it before accepting it");
        }

        if (suggestion.Withdrawn)
        {
            return ValidationResult.Error("The latest interpretation no longer finds evidence for this event. Interpret again, or reject it");
        }

        IEnumerable<EventSuggestionAccepted> events = suggestion.Status == SuggestionStatus.Accepted ? [] : [new EventSuggestionAccepted(Key)];
        return Result<IEnumerable<EventSuggestionAccepted>, ValidationResult>.Success(events);
    }
}

/// <summary>
/// A suggested event was accepted into the model.
/// </summary>
/// <param name="Key">The suggestion.</param>
[EventType]
public record EventSuggestionAccepted(SuggestionKey Key);
