// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// Holds the interpretations that wait for answers. The conversation with the language model is operational state, so it lives in memory; what the
/// person answered is recorded as events, which is why a question asked again after a restart is answered from them.
/// </summary>
public interface IInterpretationSessions
{
    /// <summary>
    /// Parks an interpretation until its questions are answered.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="parked">The interpretation.</param>
    void Park(PrologueId prologue, ParkedInterpretation parked);

    /// <summary>
    /// Takes the parked interpretation of a prologue out of the store.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The interpretation; null when none waits.</returns>
    ParkedInterpretation? Take(PrologueId prologue);

    /// <summary>
    /// Gets the questions that wait for an answer.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The questions.</returns>
    IReadOnlyList<InterpreterQuestion> Pending(PrologueId prologue);

    /// <summary>
    /// Observes the questions that wait for an answer; the current ones first.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The questions, pushed whenever they change.</returns>
    IObservable<IReadOnlyList<InterpreterQuestion>> Observe(PrologueId prologue);

    /// <summary>
    /// Gives a waiting question its answer.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="question">The question.</param>
    /// <param name="answer">The answer: the label of a choice or free text.</param>
    /// <returns>What the answer did.</returns>
    AnswerOutcome Answer(PrologueId prologue, QuestionId question, string answer);
}
