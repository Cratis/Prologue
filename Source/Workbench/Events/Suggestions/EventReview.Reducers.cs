// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Events.Reviewing;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Suggestions;

public partial record EventReview
{
    /// <summary>
    /// Reduces an interpretation finding a different set of suggestions.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static EventReview Reduce(EventSuggestionsInterpreted @event, EventReview? current, EventContext context)
    {
        var review = ReviewState.Existing(current, context);
        return review with { Suggestions = ReviewMerge.Merge(review.Suggestions, @event.Suggestions) };
    }

    /// <summary>
    /// Reduces an interpretation finding a different structure.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static EventReview Reduce(InterpretedModelRecorded @event, EventReview? current, EventContext context) =>
        ReviewState.Existing(current, context) with { Model = @event.Model };

    /// <summary>
    /// Reduces an interpretation deriving different boundaries and non-events.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static EventReview Reduce(InterpretationFindingsRecorded @event, EventReview? current, EventContext context) =>
        ReviewState.Existing(current, context) with { Boundaries = @event.Boundaries, NotEvents = @event.NotEvents };

    /// <summary>
    /// Reduces an interpretation raising different questions of its own.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static EventReview Reduce(RaisedQuestionsRecorded @event, EventReview? current, EventContext context) =>
        ReviewState.Existing(current, context) with { Raised = @event.Questions };

    /// <summary>
    /// Reduces a suggestion being accepted.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static EventReview Reduce(EventSuggestionAccepted @event, EventReview? current, EventContext context) =>
        ReviewState.Update(current, context, @event.Key, suggestion => suggestion with { Status = SuggestionStatus.Accepted });

    /// <summary>
    /// Reduces a suggestion being renamed.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static EventReview Reduce(EventSuggestionRenamed @event, EventReview? current, EventContext context) =>
        ReviewState.Update(current, context, @event.Key, suggestion => suggestion with { Name = @event.Name, Renamed = true });

    /// <summary>
    /// Reduces a suggestion being rejected.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static EventReview Reduce(EventSuggestionRejected @event, EventReview? current, EventContext context) =>
        ReviewState.Update(current, context, @event.Key, suggestion => suggestion with { Status = SuggestionStatus.Rejected });

    /// <summary>
    /// Reduces a rejected suggestion being restored.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static EventReview Reduce(EventSuggestionRestored @event, EventReview? current, EventContext context) =>
        ReviewState.Update(current, context, @event.Key, suggestion => suggestion with { Status = SuggestionStatus.Suggested });

    /// <summary>
    /// Reduces a question of the interpreter being answered.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static EventReview Reduce(InterpreterQuestionAnswered @event, EventReview? current, EventContext context)
    {
        var review = ReviewState.Existing(current, context);
        return review with { Answers = [.. review.Answers.Where(answer => answer.Key != @event.Key), new QuestionAnswer(@event.Key, @event.Prompt, @event.Answer)] };
    }

    /// <summary>
    /// Reduces a consistency boundary being decided.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static EventReview Reduce(ConsistencyBoundaryChosen @event, EventReview? current, EventContext context)
    {
        var review = ReviewState.Existing(current, context);
        return review with { Choices = [.. review.Choices.Where(choice => choice.Key != @event.Key), new BoundaryChoice(@event.Key, @event.Option, @event.Rule)] };
    }
}
