// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Discover;
using Cratis.Prologue.Workbench.Discover.Map;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Events.Reviewing;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A suggested event as the Events page shows it: the evidence, where the review stands, and whether a question of the interpreter waits on it.
/// </summary>
/// <param name="Key">The suggestion.</param>
/// <param name="Name">The name the event has now: the refined name until a person renames it.</param>
/// <param name="Mechanical">The name the heuristics gave it.</param>
/// <param name="Module">The module.</param>
/// <param name="Stream">The stream.</param>
/// <param name="Properties">The properties.</param>
/// <param name="Confidence">How strongly the evidence supports it.</param>
/// <param name="Evidence">What the suggestion rests on.</param>
/// <param name="Notes">The rules and constraints the evidence suggests.</param>
/// <param name="Status">Where it stands in the review.</param>
/// <param name="NeedsAnswer">Whether it is still to review and a question of the interpreter is about it.</param>
/// <param name="Withdrawn">Whether the last interpretation no longer finds evidence for it.</param>
/// <param name="Advice">Advice about the name; empty when it reads as a past-tense fact.</param>
/// <param name="Origin">Where the evidence comes from.</param>
[ReadModel]
public record EventCard(
    SuggestionKey Key,
    EventName Name,
    EventName Mechanical,
    ModuleName Module,
    StreamName Stream,
    IEnumerable<SuggestedProperty> Properties,
    Confidence Confidence,
    SuggestionEvidence Evidence,
    IEnumerable<SuggestionNote> Notes,
    SuggestionStatus Status,
    bool NeedsAnswer,
    bool Withdrawn,
    string Advice,
    SuggestionOrigin Origin)
{
    /// <summary>
    /// Observes the suggested events of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the review.</param>
    /// <param name="sessions">Holds the interpretation that waits for answers.</param>
    /// <returns>The events, pushed whenever the review or the questions change.</returns>
    public static ISubject<IEnumerable<EventCard>> ObserveEventCards(PrologueId id, IReadModels readModels, IInterpretationSessions sessions) =>
        new DerivedSubject<IEnumerable<EventCard>>(
            readModels.Observe<EventReview>(review => review.Id == id).StartWith([]).CombineLatest(
                sessions.Observe(id),
                (review, questions) => From(review.FirstOrDefault(), questions)));

    /// <summary>
    /// Describes the suggestions of a review as cards.
    /// </summary>
    /// <param name="review">The review; null when nothing was ever interpreted.</param>
    /// <param name="questions">The questions that wait for an answer.</param>
    /// <returns>The cards, in the order of the interpretation.</returns>
    public static IEnumerable<EventCard> From(EventReview? review, IEnumerable<Cratis.Prologue.Interpretation.InterpreterQuestion> questions)
    {
        var asked = questions.SelectMany(question => QuestionLinks.Affected(question, review?.Suggestions ?? [])).ToHashSet();
        return
        [
            .. (review?.Suggestions ?? []).Select(reviewed => new EventCard(
                reviewed.Suggestion.Key,
                reviewed.Name,
                reviewed.Suggestion.Mechanical,
                reviewed.Suggestion.Module,
                reviewed.Suggestion.Stream,
                reviewed.Suggestion.Properties,
                reviewed.Suggestion.Confidence,
                reviewed.Suggestion.Evidence,
                reviewed.Suggestion.Notes,
                reviewed.Status,
                reviewed.Status == SuggestionStatus.Suggested && asked.Contains(reviewed.Suggestion.Key),
                reviewed.Withdrawn,
                EventNames.Advice(reviewed.Name.Value),
                reviewed.Suggestion.Origin))
        ];
    }
}
