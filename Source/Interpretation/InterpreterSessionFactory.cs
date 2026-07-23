// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents an <see cref="IInterpreterSessionFactory"/> creating <see cref="InterpreterSession"/> instances.
/// </summary>
/// <param name="heuristics">The deterministic model builder sessions build their structure with.</param>
/// <param name="chatClients">The factory creating the chat client when a session refines.</param>
/// <param name="logger">The logger sessions log through.</param>
public class InterpreterSessionFactory(
    IBuildHeuristicModel heuristics,
    IChatClientFactory chatClients,
    ILogger<InterpreterSession> logger) : IInterpreterSessionFactory
{
    /// <inheritdoc/>
    public IInterpreterSession CreateNew(
        Guid prologueId,
        IReadOnlyList<Capture> captures,
        LlmOptions llmOptions,
        Action<InterpreterStatus>? statusChanged = null,
        int maxQuestionRounds = IInterpreterSessionFactory.DefaultMaxQuestionRounds) =>
        new InterpreterSession(InterpreterSessionState.New(prologueId), captures, llmOptions, heuristics, chatClients, logger, statusChanged, maxQuestionRounds);

    /// <inheritdoc/>
    public IInterpreterSession Resume(
        InterpreterSessionState state,
        IReadOnlyList<Capture> captures,
        LlmOptions llmOptions,
        Action<InterpreterStatus>? statusChanged = null,
        int maxQuestionRounds = IInterpreterSessionFactory.DefaultMaxQuestionRounds) =>
        new InterpreterSession(state, captures, llmOptions, heuristics, chatClients, logger, statusChanged, maxQuestionRounds);
}
