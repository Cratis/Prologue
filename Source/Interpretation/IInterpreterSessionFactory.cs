// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Creates interpreter sessions — new ones from a Prologue's captures, and resumed ones from a previously
/// serialized <see cref="InterpreterSessionState"/>. The language model is stateless, so resuming needs nothing
/// beyond the state (whose transcript is replayed) and the captures.
/// </summary>
public interface IInterpreterSessionFactory
{
    /// <summary>
    /// The default maximum number of ask-and-answer rounds a session runs before it finalizes with whatever the
    /// language model last returned.
    /// </summary>
    const int DefaultMaxQuestionRounds = 3;

    /// <summary>
    /// Creates a new session for a Prologue's captures.
    /// </summary>
    /// <param name="prologueId">The Prologue the captures belong to.</param>
    /// <param name="captures">The correlated captures to interpret.</param>
    /// <param name="llmOptions">The language-model configuration; refinement is skipped entirely when disabled.</param>
    /// <param name="statusChanged">An optional callback invoked on every status transition.</param>
    /// <param name="maxQuestionRounds">The maximum number of ask-and-answer rounds; zero makes the session non-interactive — the prompt tells the model not to ask and the session finalizes instead of awaiting answers.</param>
    /// <returns>The new <see cref="IInterpreterSession"/>.</returns>
    IInterpreterSession CreateNew(
        Guid prologueId,
        IReadOnlyList<Capture> captures,
        LlmOptions llmOptions,
        Action<InterpreterStatus>? statusChanged = null,
        int maxQuestionRounds = DefaultMaxQuestionRounds);

    /// <summary>
    /// Resumes a session from a previously serialized state — mid-conversation resumes replay the state's
    /// transcript to the stateless language model.
    /// </summary>
    /// <param name="state">The state to resume from.</param>
    /// <param name="captures">The correlated captures the session interprets.</param>
    /// <param name="llmOptions">The language-model configuration; refinement is skipped entirely when disabled.</param>
    /// <param name="statusChanged">An optional callback invoked on every status transition.</param>
    /// <param name="maxQuestionRounds">The maximum number of ask-and-answer rounds; zero makes the session non-interactive — the prompt tells the model not to ask and the session finalizes instead of awaiting answers.</param>
    /// <returns>The resumed <see cref="IInterpreterSession"/>.</returns>
    IInterpreterSession Resume(
        InterpreterSessionState state,
        IReadOnlyList<Capture> captures,
        LlmOptions llmOptions,
        Action<InterpreterStatus>? statusChanged = null,
        int maxQuestionRounds = DefaultMaxQuestionRounds);
}
