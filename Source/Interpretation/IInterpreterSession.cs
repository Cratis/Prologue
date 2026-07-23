// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Defines a resumable interpreter session — the engine that turns a Prologue's captures into an event model,
/// asking the user questions along the way when the language model is genuinely uncertain. The session runs to a
/// checkpoint on every <see cref="Proceed"/>, and its <see cref="State"/> is fully serializable, so a synchronous
/// host loops in place while an asynchronous host persists the state and resumes any time later.
/// </summary>
public interface IInterpreterSession
{
    /// <summary>
    /// Gets the current state of the session.
    /// </summary>
    InterpreterSessionState State { get; }

    /// <summary>
    /// Runs the session until it reaches <see cref="InterpreterStatus.Completed"/>,
    /// <see cref="InterpreterStatus.Failed"/>, or <see cref="InterpreterStatus.AwaitingAnswers"/>. Idempotent —
    /// calling it again after the pending questions are answered continues the refinement, and calling it on a
    /// finished session returns the state unchanged.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The state the session checkpointed at.</returns>
    Task<InterpreterSessionState> Proceed(CancellationToken cancellationToken = default);

    /// <summary>
    /// Records the answer for one pending question. When every pending question is answered, the next
    /// <see cref="Proceed"/> continues the refinement with the answers.
    /// </summary>
    /// <param name="answer">The answer to record.</param>
    /// <returns>The state after recording the answer.</returns>
    /// <exception cref="QuestionNotPending">Thrown when the answer refers to a question that is not pending.</exception>
    InterpreterSessionState Answer(InterpreterAnswer answer);
}
