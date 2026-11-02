// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// Answers a question the interpreter asked. The answer goes back into the interpretation, which carries on and re-interprets what the answer
/// changes once every question is answered. The answer is recorded, so the same question is answered from it the next time it is asked.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Question">The question, as it waits in the page.</param>
/// <param name="Answer">The label of a choice, or free text.</param>
[Command]
public record AnswerInterpreterQuestion(PrologueId Prologue, QuestionId Question, string Answer)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="sessions">Holds the interpretation that waits for the answer.</param>
    /// <param name="queue">Queues the interpretation to carry on.</param>
    /// <param name="review">The review, which holds the questions the Workbench raised itself.</param>
    /// <returns>The recorded answer, or a refusal.</returns>
    public Result<InterpreterQuestionAnswered, ValidationResult> Handle(IInterpretationSessions sessions, IInterpretationQueue queue, [Unprotected("Only reads which questions the Workbench raised; an answer to one that was answered meanwhile is recorded again with the same meaning")] EventReview? review)
    {
        if (string.IsNullOrWhiteSpace(Answer))
        {
            return ValidationResult.Error("Write or pick an answer");
        }

        var outcome = sessions.Answer(Prologue, Question, Answer.Trim());
        if (outcome.Question is null && RaisedQuestions.Find(review, Question) is { } raised)
        {
            return new InterpreterQuestionAnswered(raised.Key, raised.Prompt, Answer.Trim());
        }

        if (outcome.Question is null)
        {
            return ValidationResult.Error("This question is not waiting for an answer anymore. Interpret again to get the current ones");
        }

        if (outcome.Complete)
        {
            queue.Enqueue(new InterpretationJob(Prologue, true));
        }

        return new InterpreterQuestionAnswered(QuestionKeys.For(outcome.Question.Prompt), outcome.Question.Prompt, Answer.Trim());
    }
}

/// <summary>
/// A question of the interpreter was answered.
/// </summary>
/// <param name="Key">The question, by its normalized prompt.</param>
/// <param name="Prompt">The question as it was asked.</param>
/// <param name="Answer">The answer.</param>
[EventType]
public record InterpreterQuestionAnswered(QuestionKey Key, string Prompt, string Answer);
