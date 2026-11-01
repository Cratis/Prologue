// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Abstractions;
using Cratis.AI.Agents;
using Cratis.AI.LanguageModels;

namespace Cratis.Prologue.Workbench.AI;

/// <summary>
/// Adapts the <see cref="PrologueAgents"/> catalog onto the seam Cratis.AI resolves an agent's identity through.
/// </summary>
public class PrologueAIAgents : IAIAgents
{
    /// <inheritdoc/>
    public AgentDescriptor? Find(AgentId id)
    {
        var agent = PrologueAgents.All.FirstOrDefault(_ => AgentId.For(_.Purpose) == id);
        return agent is null ? null : ToDescriptor(agent);
    }

    /// <inheritdoc/>
    public AgentDescriptor? FindByPurpose(LanguageModelPurpose purpose)
    {
        var agent = PrologueAgents.All.FirstOrDefault(_ => _.Purpose == purpose);
        return agent is null ? null : ToDescriptor(agent);
    }

    static AgentDescriptor ToDescriptor(PrologueAgent agent) => new(AgentId.For(agent.Purpose), new AgentName(agent.Name));
}
