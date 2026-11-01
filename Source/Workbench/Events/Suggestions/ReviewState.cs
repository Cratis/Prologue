// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.NotEvents;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// The steps the reducers of the review share.
/// </summary>
internal static class ReviewState
{
    internal static EventReview Existing(EventReview? current, EventContext context) =>
        current ?? new(new PrologueId(Guid.Parse(context.EventSourceId.Value)), [], InterpretedModel.Empty, [], [], [], []);

    internal static EventReview Update(EventReview? current, EventContext context, SuggestionKey key, Func<ReviewedSuggestion, ReviewedSuggestion> change)
    {
        var review = Existing(current, context);
        return review with { Suggestions = [.. review.Suggestions.Select(suggestion => suggestion.Suggestion.Key == key ? change(suggestion) : suggestion)] };
    }
}
