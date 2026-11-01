// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Providers;
using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.Projections.ModelBound;
using Cratis.Prologue.Workbench.AI.Pools.Arranging;
using MongoDB.Driver;

namespace Cratis.Prologue.Workbench.AI.Pools;

/// <summary>
/// What the Workbench adds to a Cratis.AI provider pool: the capability tier it is for, the priority order of its
/// members and how it picks among them. Keyed by the pool's id, so the AI panel pairs it with the pool and its members
/// from Cratis.AI.
/// </summary>
/// <param name="Id">The pool's id.</param>
/// <param name="Tier">The <see cref="ModelTier"/> the pool is for.</param>
/// <param name="Order">The members in priority order, first choice first.</param>
/// <param name="Ordering">The <see cref="PoolOrdering"/>.</param>
/// <param name="Fallback">The <see cref="PoolFallback"/>.</param>
[ReadModel]
public record PoolArrangement(
    string Id,
    [SetFrom<PoolArranged>(nameof(PoolArranged.Tier))]
    ModelTier Tier,
    [SetFrom<PoolArranged>(nameof(PoolArranged.Order))]
    IEnumerable<AIProviderId> Order,
    [SetFrom<PoolArranged>(nameof(PoolArranged.Ordering))]
    PoolOrdering Ordering,
    [SetFrom<PoolArranged>(nameof(PoolArranged.Fallback))]
    PoolFallback Fallback)
{
    /// <summary>
    /// Observes the arrangement of every pool that has one.
    /// </summary>
    /// <param name="collection">The projected arrangements.</param>
    /// <returns>The arrangements.</returns>
    public static ISubject<IEnumerable<PoolArrangement>> AllPoolArrangements(IMongoCollection<PoolArrangement> collection) =>
        collection.Observe();
}
