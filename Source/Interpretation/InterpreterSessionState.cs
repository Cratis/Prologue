// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents the complete, serializable state of an interpreter session — the unit a host persists at a
/// checkpoint and hands back to the session factory to resume any time later. It round-trips cleanly through
/// System.Text.Json, and because the transcript carries the full language-model conversation, resuming
/// mid-conversation needs nothing beyond this state and the captures.
/// </summary>
/// <param name="PrologueId">The Prologue the session interprets captures for.</param>
/// <param name="Status">The stage the session is at.</param>
/// <param name="Model">The event model so far — the heuristic model once built, the refined model when completed; <see langword="null"/> before the model is built.</param>
/// <param name="PendingQuestions">The questions the language model asked that have not been answered yet.</param>
/// <param name="AnsweredQuestions">Every question that has been answered, in the order the answers arrived.</param>
/// <param name="Transcript">The language-model conversation so far — replayed to the stateless model on resume.</param>
/// <param name="QuestionRounds">The number of completed ask-and-answer rounds.</param>
/// <param name="Error">What went wrong; empty unless <paramref name="Status"/> is <see cref="InterpreterStatus.Failed"/>.</param>
public record InterpreterSessionState(
    Guid PrologueId,
    InterpreterStatus Status,
    ExtractionResult? Model,
    IReadOnlyList<InterpreterQuestion> PendingQuestions,
    IReadOnlyList<AnsweredQuestion> AnsweredQuestions,
    IReadOnlyList<SessionChatMessage> Transcript,
    int QuestionRounds,
    string Error)
{
    /// <summary>
    /// Creates the initial state for a new session.
    /// </summary>
    /// <param name="prologueId">The Prologue the session interprets captures for.</param>
    /// <returns>An <see cref="InterpreterSessionState"/> that has not started.</returns>
    public static InterpreterSessionState New(Guid prologueId) =>
        new(prologueId, InterpreterStatus.NotStarted, null, [], [], [], 0, string.Empty);
}
