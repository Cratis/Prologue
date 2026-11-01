// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Reviewing;

/// <summary>
/// Rejects a suggested event. It stays in the review, out of the model, until it is restored. Rejecting what is rejected records nothing.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Key">The suggestion.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record RejectSuggestion(PrologueId Prologue, SuggestionKey Key)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="review">The review, read under the command's consistency boundary.</param>
    /// <returns>The event, nothing, or a refusal.</returns>
    public Result<IEnumerable<EventSuggestionRejected>, ValidationResult> Handle(EventReview? review)
    {
        if (ReviewRules.Find(review, Key, out var suggestion) is { } missing)
        {
            return missing;
        }

        IEnumerable<EventSuggestionRejected> events = suggestion.Status == SuggestionStatus.Rejected ? [] : [new EventSuggestionRejected(Key)];
        return Result<IEnumerable<EventSuggestionRejected>, ValidationResult>.Success(events);
    }
}

/// <summary>
/// A suggested event was rejected.
/// </summary>
/// <param name="Key">The suggestion.</param>
[EventType]
public record EventSuggestionRejected(SuggestionKey Key);
