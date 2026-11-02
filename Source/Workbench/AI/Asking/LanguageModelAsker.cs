// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Conversations;
using Cratis.Prologue.Workbench.AI.Dispatch;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.AI.Asking;

/// <summary>
/// Asks an agent through its chat client, which the Workbench's dispatch sends to the members of the agent's pool in the recorded order,
/// under the pool's fallback and ceiling.
/// </summary>
/// <param name="clients">Hands out the chat client of an agent.</param>
/// <param name="logger">The logger a failure to ask is reported to.</param>
public class LanguageModelAsker(IAIChatClients clients, ILogger<LanguageModelAsker> logger) : IAgentAsker
{
    /// <inheritdoc/>
    public async Task<AgentReply> Ask(PrologueAgent agent, string prompt, CancellationToken cancellationToken)
    {
        try
        {
            using var client = clients.For(agent.Purpose);
            var response = await client.GetResponseAsync(prompt, cancellationToken: cancellationToken);
            return AgentReply.Answered(response.Text);
        }
        catch (AgentDispatchStopped stopped)
        {
            logger.CouldNotAsk(agent.Name, stopped.Message);
            return AgentReply.StoppedBy(stopped);
        }
        catch (AIChatClientUnavailable unavailable)
        {
            logger.CouldNotAsk(agent.Name, unavailable.Reason);
            return AgentReply.Unavailable(unavailable.Reason);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.AskingFailed(agent.Name, exception);
            return AgentReply.Unavailable("The agent could not be reached");
        }
    }
}
