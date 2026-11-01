// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.LanguageModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.AI.Asking;

/// <summary>
/// Asks an agent through the Cratis.AI language model, which resolves the agent's provider or pool from its configuration.
/// </summary>
/// <param name="services">Resolves the language model when it is asked for, so a host without one still starts.</param>
/// <param name="logger">The logger a failure to ask is reported to.</param>
public class LanguageModelAsker(IServiceProvider services, ILogger<LanguageModelAsker> logger) : IAgentAsker
{
    /// <inheritdoc/>
    public async Task<AgentReply> Ask(PrologueAgent agent, string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var model = services.GetService<ILanguageModel>();
            if (model is null)
            {
                return AgentReply.Unavailable("No language model is registered for this Workbench");
            }

            var result = await model.Complete(prompt, agent.Purpose, cancellationToken);
            if (result.Succeeded)
            {
                return AgentReply.Answered(result.Text);
            }

            var reason = string.IsNullOrWhiteSpace(result.FailureReason) ? "The agent did not answer" : result.FailureReason;
            logger.CouldNotAsk(agent.Name, reason);
            return AgentReply.Unavailable(reason);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.AskingFailed(agent.Name, exception);
            return AgentReply.Unavailable("The agent could not be reached");
        }
    }
}
