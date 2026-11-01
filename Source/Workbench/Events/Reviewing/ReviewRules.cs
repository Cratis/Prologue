// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Reviewing;

/// <summary>
/// The rules the commands that review suggestions share.
/// </summary>
public static class ReviewRules
{
    /// <summary>
    /// Finds the suggestion a review command is about, or says why there is none.
    /// </summary>
    /// <param name="review">The review.</param>
    /// <param name="key">The suggestion.</param>
    /// <param name="suggestion">The suggestion, when it exists.</param>
    /// <returns>The refusal, or null when the suggestion exists.</returns>
    public static ValidationResult? Find(EventReview? review, SuggestionKey key, out ReviewedSuggestion suggestion)
    {
        suggestion = EventReview.Find(review, key)!;
        return suggestion is null ? ValidationResult.Error("No event is suggested under that key") : null;
    }

    /// <summary>
    /// Gets why a name cannot be chosen for a suggestion, when it cannot.
    /// </summary>
    /// <param name="review">The review.</param>
    /// <param name="key">The suggestion being renamed.</param>
    /// <param name="name">The name.</param>
    /// <returns>The refusal, or null when the name can be used.</returns>
    public static ValidationResult? RefuseName(EventReview review, SuggestionKey key, EventName name)
    {
        if (string.IsNullOrWhiteSpace(name.Value))
        {
            return ValidationResult.Error("Give the event a name");
        }

        if (!EventNames.IsPascalCase(name.Value))
        {
            return ValidationResult.Error("An event name is PascalCase: start with a capital letter and use letters and digits only");
        }

        var taken = review.Suggestions.Any(other =>
            other.Suggestion.Key != key &&
            other.Status != SuggestionStatus.Rejected &&
            !other.Withdrawn &&
            other.Name == name);
        return taken ? ValidationResult.Error($"Another event is already named {name.Value}") : null;
    }
}
