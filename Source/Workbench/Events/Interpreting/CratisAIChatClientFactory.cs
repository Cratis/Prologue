// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Conversations;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.AI;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Represents an <see cref="IChatClientFactory"/> that backs the Interpretation library's refinement with Cratis.AI: the chat goes to the Interpreter
/// agent's provider or pool, with the dispatch, limits and usage recording the Workbench gives every agent. The batch Interpreter keeps its own
/// configured language model; only the Workbench uses this.
/// </summary>
/// <param name="clients">Hands out the chat client of an agent.</param>
public class CratisAIChatClientFactory(IAIChatClients clients) : IChatClientFactory, ILanguageModelWatch
{
    /// <inheritdoc/>
    public string Failure { get; private set; } = string.Empty;

    /// <inheritdoc/>
    public IChatClient CreateFor(LlmOptions options)
    {
        try
        {
            return new WatchedChatClient(clients.For(PrologueAgents.Interpreter.Purpose), reason => Failure = reason);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Failure = WatchedChatClient.Describe(exception);
            throw;
        }
    }
}
