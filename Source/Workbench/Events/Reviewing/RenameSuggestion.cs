// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Reviewing;

/// <summary>
/// Gives a suggested event the name a person chose. The name must be PascalCase and not taken by another event; a name that does not read
/// as a past-tense fact is allowed, and the review shows advice for it. Choosing the name the event has records nothing.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Key">The suggestion.</param>
/// <param name="Name">The name.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record RenameSuggestion(PrologueId Prologue, SuggestionKey Key, EventName Name)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="review">The review, read under the command's consistency boundary.</param>
    /// <returns>The event, nothing, or a refusal.</returns>
    public Result<IEnumerable<EventSuggestionRenamed>, ValidationResult> Handle(EventReview? review)
    {
        if (ReviewRules.Find(review, Key, out var suggestion) is { } missing)
        {
            return missing;
        }

        var name = new EventName(Name.Value.Trim());
        if (ReviewRules.RefuseName(review!, Key, name) is { } refusal)
        {
            return refusal;
        }

        IEnumerable<EventSuggestionRenamed> events = suggestion.Name == name ? [] : [new EventSuggestionRenamed(Key, name)];
        return Result<IEnumerable<EventSuggestionRenamed>, ValidationResult>.Success(events);
    }
}

/// <summary>
/// A suggested event was given a name a person chose.
/// </summary>
/// <param name="Key">The suggestion.</param>
/// <param name="Name">The name.</param>
[EventType]
public record EventSuggestionRenamed(SuggestionKey Key, EventName Name);
