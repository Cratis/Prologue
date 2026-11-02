// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Discover.Map;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// A question the interpreter asked that waits for an answer.
/// </summary>
/// <param name="Id">The question in this interpretation; pass it to <see cref="AnswerInterpreterQuestion"/>.</param>
/// <param name="Key">The question across interpretations.</param>
/// <param name="Prompt">The question.</param>
/// <param name="Context">Why the interpreter is unsure.</param>
/// <param name="Choices">The choices; the page always adds free text.</param>
/// <param name="Affects">The suggestions the question is about.</param>
/// <param name="Standalone">Whether the Workbench raised the question from its own evidence; answering it needs a new interpretation, because no interpretation waits for it.</param>
[ReadModel]
public record OpenQuestion(QuestionId Id, QuestionKey Key, string Prompt, string Context, IEnumerable<QuestionOption> Choices, IEnumerable<SuggestionKey> Affects, bool Standalone)
{
    /// <summary>
    /// Observes the questions that wait for an answer.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the review.</param>
    /// <param name="sessions">Holds the interpretation that waits.</param>
    /// <returns>The questions, pushed whenever they change.</returns>
    public static ISubject<IEnumerable<OpenQuestion>> ObserveOpenQuestions(PrologueId id, IReadModels readModels, IInterpretationSessions sessions) =>
        new DerivedSubject<IEnumerable<OpenQuestion>>(
            sessions.Observe(id).CombineLatest(
                readModels.Observe<EventReview>(review => review.Id == id).StartWith([]),
                (questions, review) => From(questions, review.FirstOrDefault())));

    /// <summary>
    /// Describes the questions that wait, with the suggestions each is about.
    /// </summary>
    /// <param name="questions">The questions.</param>
    /// <param name="review">The review; null when nothing was ever interpreted.</param>
    /// <returns>The open questions.</returns>
    public static IEnumerable<OpenQuestion> From(IEnumerable<InterpreterQuestion> questions, EventReview? review) =>
    [
        .. questions.Select(question => new OpenQuestion(
            question.Id,
            QuestionKeys.For(question.Prompt),
            question.Prompt,
            question.Context,
            [.. question.Choices.Select(choice => new QuestionOption(choice.Label, choice.Description))],
            [.. QuestionLinks.Affected(question, review?.Suggestions ?? [])],
            false)),
        .. RaisedQuestions.Open(review).Select(raised => new OpenQuestion(raised.Id, raised.Key, raised.Prompt, raised.Context, [.. raised.Choices], [], true))
    ];
}
