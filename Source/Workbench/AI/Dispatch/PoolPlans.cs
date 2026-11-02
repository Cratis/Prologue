// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Conversations;
using Cratis.AI.LanguageModels;
using Cratis.AI.Providers;
using Cratis.AI.Providers.Capacity;
using Cratis.AI.Providers.Listing;
using Cratis.AI.Providers.Pools.Listing;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.AI.Network;
using Cratis.Prologue.Workbench.AI.Pools;
using Cratis.Prologue.Workbench.AI.Pools.Ceiling;
using Cratis.Prologue.Workbench.AI.Pools.Spending;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Plans a dispatch from Cratis.AI's agent, pool, provider and capacity facts and the Workbench's own arrangement, network marks, ceiling and spend.
/// </summary>
/// <param name="readModels">Reads the facts.</param>
/// <param name="capacities">Reports each provider's remaining capacity.</param>
/// <param name="time">Tells the day.</param>
public class PoolPlans(IReadModels readModels, IAIProviderCapacities capacities, TimeProvider time) : IPoolPlans
{
    /// <summary>
    /// Gets the UTC day a spend belongs to, as yyyy-MM-dd.
    /// </summary>
    /// <param name="time">The clock.</param>
    /// <returns>The day.</returns>
    public static string DayOf(TimeProvider time) => time.GetUtcNow().ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public async Task<PoolPlan?> For(LanguageModelPurpose purpose, CancellationToken cancellationToken)
    {
        var agent = await FindAgent(purpose);
        if (agent?.PoolId is null)
        {
            return null;
        }

        var key = agent.PoolId.Value.ToString();
        var pool = await readModels.GetInstanceById<AIProviderPool>((EventSourceId)key) ?? throw new PoolNotReady(agent.Name.Value, key);

        var arrangement = await readModels.GetInstanceById<PoolArrangement>((EventSourceId)key);
        var ceiling = await readModels.GetInstanceById<PoolCeiling>((EventSourceId)key);
        var spend = await readModels.GetInstanceById<PoolSpend>((EventSourceId)key);
        var listed = (pool.Members ?? []).Select(member => member.ProviderId).ToList();

        var states = new Dictionary<AIProviderId, AIProviderCapacity>();
        foreach (var provider in listed)
        {
            states[provider] = await capacities.For(provider, cancellationToken);
        }

        var order = PoolOrder.Of(listed, arrangement?.Order ?? [], arrangement?.Ordering ?? PoolOrdering.ByRemainingCapacity, provider => states[provider].Headroom);
        var members = new List<DispatchMember>();
        foreach (var provider in order)
        {
            members.Add(await MemberFor(provider, states[provider]));
        }

        var target = new ChatAgentTarget(agent.Name.Value, agent.Description.Value, agent.Tier, null, null, agent.Skills);
        return new(key, pool.Name.Value, arrangement?.Fallback ?? PoolFallback.ContinueOnNext, members, target, ceiling?.TokensPerDay.Value ?? 0, spend?.On(DayOf(time)) ?? 0);
    }

    /// <summary>
    /// Finds the agent behind a purpose.
    /// </summary>
    /// <param name="purpose">The purpose, the agent's identity.</param>
    /// <returns>The agent; null when none is known.</returns>
    protected virtual Task<Cratis.AI.Agents.Listing.Agent?> FindAgent(LanguageModelPurpose purpose) => Cratis.AI.Agents.Listing.Agent.For(readModels, purpose);

    async Task<DispatchMember> MemberFor(AIProviderId id, AIProviderCapacity capacity)
    {
        var key = (EventSourceId)id.Value.ToString();
        var provider = await readModels.GetInstanceById<AIProvider>(key);
        var mark = await readModels.GetInstanceById<ProviderNetworkPosition>(key);
        var inside = provider is not null && NetworkPositions.IsInside(provider.Type == AIProviderType.OpenAICompatible, provider.Endpoint?.Value, mark?.IsInside);
        var problem = capacity.CanStartWork ? string.Empty : capacity.Explain();
        return new(id, provider?.Name.Value ?? "Unknown provider", inside, capacity.Headroom, capacity.CanStartWork, problem);
    }
}
