// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.NotEvents;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Decides what of an interpretation is worth recording: what differs from the last one. Looking again is not a fact.
/// </summary>
public static class InterpretationDecision
{
    /// <summary>
    /// Decides the events.
    /// </summary>
    /// <param name="current">The review as it is now; null when nothing was ever interpreted.</param>
    /// <param name="suggestions">The events the interpretation suggests.</param>
    /// <param name="model">The structure the interpretation found.</param>
    /// <param name="boundaries">The consistency boundaries the evidence raises.</param>
    /// <param name="notEvents">What the evidence shows is not an event.</param>
    /// <param name="questions">The questions the Workbench raises from its own evidence.</param>
    /// <returns>The events; none when nothing differs.</returns>
    public static IEnumerable<object> Decide(
        EventReview? current,
        IEnumerable<SuggestedEvent> suggestions,
        InterpretedModel model,
        IEnumerable<ConsistencyBoundary> boundaries,
        IEnumerable<NotEvent> notEvents,
        IEnumerable<RaisedQuestion> questions)
    {
        return Suggested(current, suggestions)
            .Concat(Modelled(current, model))
            .Concat(Findings(current, boundaries, notEvents))
            .Concat(Raised(current, questions));
    }

    static IEnumerable<object> Suggested(EventReview? current, IEnumerable<SuggestedEvent> suggestions)
    {
        var found = suggestions.ToList();
        var known = current?.Suggestions.Where(suggestion => !suggestion.Withdrawn).Select(suggestion => suggestion.Suggestion) ?? [];
        if (!Same(known, found))
        {
            yield return new EventSuggestionsInterpreted(found);
        }
    }

    static IEnumerable<object> Modelled(EventReview? current, InterpretedModel model)
    {
        if (!Same(current?.Model ?? InterpretedModel.Empty, model))
        {
            yield return new InterpretedModelRecorded(model);
        }
    }

    static IEnumerable<object> Findings(EventReview? current, IEnumerable<ConsistencyBoundary> boundaries, IEnumerable<NotEvent> notEvents)
    {
        var derived = boundaries.ToList();
        var notes = notEvents.ToList();
        if (!Same(current?.Boundaries ?? [], derived) || !Same(current?.NotEvents ?? [], notes))
        {
            yield return new InterpretationFindingsRecorded(derived, notes);
        }
    }

    static IEnumerable<object> Raised(EventReview? current, IEnumerable<RaisedQuestion> questions)
    {
        var raised = questions.ToList();
        if (!SameQuestions(current?.Raised ?? [], raised))
        {
            yield return new RaisedQuestionsRecorded(raised);
        }
    }

    // A question is the same question while it is about the same thing and offers the same answers. Its prompt and context carry counts that move with every
    // capture, and recording the question again for a count would append an event for looking, not for a change.
    static bool SameQuestions(IEnumerable<RaisedQuestion> left, IEnumerable<RaisedQuestion> right) =>
        Same(left.Select(Identity), right.Select(Identity));

    static QuestionIdentity Identity(RaisedQuestion question) => new(question.Key.Value, question.Choices);

    static bool Same<T>(IEnumerable<T> left, IEnumerable<T> right) => JsonSerializer.Serialize(left) == JsonSerializer.Serialize(right);

    static bool Same(InterpretedModel left, InterpretedModel right) => JsonSerializer.Serialize(left) == JsonSerializer.Serialize(right);

    sealed record QuestionIdentity(string Key, IEnumerable<QuestionOption> Choices);
}
