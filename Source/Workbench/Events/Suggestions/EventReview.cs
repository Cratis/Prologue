// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.NotEvents;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// Everything the review of a prologue's events rests on and has decided: the suggestions with their status, the structure of the last
/// interpretation, the answers given to the interpreter, the derived consistency boundaries with the choices made, and what is not an event.
/// The commands that review read it under their consistency boundary. Read the pages from the queries built on it, not from this.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Suggestions">The suggestions with where each stands.</param>
/// <param name="Model">The structure the last interpretation found.</param>
/// <param name="Answers">The answers given to the interpreter.</param>
/// <param name="Boundaries">The consistency boundaries derived from the evidence.</param>
/// <param name="Choices">The options chosen for the boundaries.</param>
/// <param name="NotEvents">What the evidence shows is not an event.</param>
[ReadModel]
public partial record EventReview(
    PrologueId Id,
    IEnumerable<ReviewedSuggestion> Suggestions,
    InterpretedModel Model,
    IEnumerable<QuestionAnswer> Answers,
    IEnumerable<ConsistencyBoundary> Boundaries,
    IEnumerable<BoundaryChoice> Choices,
    IEnumerable<NotEvent> NotEvents)
{
    /// <summary>
    /// Observes the review of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored review.</param>
    /// <returns>The review, pushed again whenever it changes.</returns>
    public static ISubject<EventReview> ObserveEventReview(PrologueId id, IReadModels readModels) =>
        readModels.Observe<EventReview>(review => review.Id == id).FirstItem();

    /// <summary>
    /// Finds a suggestion in the review.
    /// </summary>
    /// <param name="review">The review, or null when nothing was ever interpreted.</param>
    /// <param name="key">The suggestion.</param>
    /// <returns>The suggestion, or null when the review does not hold it.</returns>
    public static ReviewedSuggestion? Find(EventReview? review, SuggestionKey key) =>
        review?.Suggestions.FirstOrDefault(suggestion => suggestion.Suggestion.Key == key);
}
