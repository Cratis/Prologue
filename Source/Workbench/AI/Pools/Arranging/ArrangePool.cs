// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Providers;
using Cratis.AI.Providers.Pools;
using Cratis.Arc.Chronicle.ReadModels;

namespace Cratis.Prologue.Workbench.AI.Pools.Arranging;

/// <summary>
/// Command for arranging a provider pool: the tier it is for, the order of its members and how it picks among them.
/// Cratis.AI keeps the pool and its members; the arrangement is the Workbench's own fact about it, and what the network
/// posture is derived from.
/// </summary>
/// <param name="Pool">The <see cref="AIProviderPoolId"/>.</param>
/// <param name="Tier">The <see cref="ModelTier"/> the pool is for.</param>
/// <param name="Order">The members, first choice first.</param>
/// <param name="Ordering">The <see cref="PoolOrdering"/>.</param>
/// <param name="Fallback">The <see cref="PoolFallback"/>.</param>
[Command]
public record ArrangePool(AIProviderPoolId Pool, ModelTier Tier, IEnumerable<AIProviderId> Order, PoolOrdering Ordering, PoolFallback Fallback) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Pool.Value.ToString();

    /// <summary>
    /// Handles the command. Arranging the pool the way it already is records nothing.
    /// </summary>
    /// <param name="current">The pool's arrangement as it stands, if it has one.</param>
    /// <returns>The event, or nothing when the arrangement does not change.</returns>
    public IEnumerable<PoolArranged> Handle([Unprotected("Arranging twice at once keeps the last arrangement; the guard only avoids repeating the same fact")] PoolArrangement? current)
    {
        var order = Order.Distinct().ToArray();
        var unchanged = current is not null
            && current.Tier == Tier
            && current.Ordering == Ordering
            && current.Fallback == Fallback
            && current.Order.SequenceEqual(order);
        return unchanged ? [] : [new PoolArranged(Tier, order, Ordering, Fallback)];
    }
}

/// <summary>
/// Event raised when a provider pool was arranged.
/// </summary>
/// <param name="Tier">The <see cref="ModelTier"/> the pool is for.</param>
/// <param name="Order">The members, first choice first.</param>
/// <param name="Ordering">The <see cref="PoolOrdering"/>.</param>
/// <param name="Fallback">The <see cref="PoolFallback"/>.</param>
[EventType]
public record PoolArranged(ModelTier Tier, IEnumerable<AIProviderId> Order, PoolOrdering Ordering, PoolFallback Fallback);
