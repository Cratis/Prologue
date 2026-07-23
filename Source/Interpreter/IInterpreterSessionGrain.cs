// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Defines the grain hosting one Prologue's interpreter session, keyed by the Prologue identifier. The session's
/// state persists at every checkpoint, so the grain — and the whole container — can disappear at any time and a
/// later activation resumes mid-conversation from the persisted state.
/// </summary>
public interface IInterpreterSessionGrain : IGrainWithGuidKey
{
    /// <summary>
    /// Starts the session, or continues it when it is already running — idempotent. Interpretation proceeds in
    /// the background; poll <see cref="GetStatus"/> for progress.
    /// </summary>
    /// <param name="options">The <see cref="LlmOptions"/> for the session's language model.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Start(LlmOptions options);

    /// <summary>
    /// Gets the snapshot of the session as it stands.
    /// </summary>
    /// <returns>The <see cref="SessionSnapshot"/>.</returns>
    Task<SessionSnapshot> GetStatus();

    /// <summary>
    /// Records the answer for one pending question and continues the session in the background when every pending
    /// question has been answered.
    /// </summary>
    /// <param name="answer">The <see cref="InterpreterAnswer"/> to record.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="QuestionNotPending">Thrown when the answer refers to a question that is not pending.</exception>
    Task Answer(InterpreterAnswer answer);

    /// <summary>
    /// Gets the session's result once it has completed — the extracted event model and the generated Screenplay.
    /// </summary>
    /// <returns>The <see cref="SessionResult"/>, or <see langword="null"/> until the session has completed.</returns>
    Task<SessionResult?> GetResult();
}
