// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Asking;

/// <summary>
/// Asks one of the Prologue agents a question. The agent runs on the provider or pool Cratis.AI has configured for it; the
/// Workbench never calls a vendor itself.
/// </summary>
public interface IAgentAsker
{
    /// <summary>
    /// Asks an agent to complete a prompt.
    /// </summary>
    /// <param name="agent">The agent to ask.</param>
    /// <param name="prompt">The prompt.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The reply; never throws for an agent that cannot answer.</returns>
    Task<AgentReply> Ask(PrologueAgent agent, string prompt, CancellationToken cancellationToken);
}
