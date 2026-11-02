// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.Projections.ModelBound;
using MongoDB.Driver;

namespace Cratis.Prologue.Workbench.AI.Pools.Ceiling;

/// <summary>
/// The most tokens a provider pool may use in a day (UTC), which the Workbench's dispatcher enforces. Cratis.AI has no pool-wide
/// ceiling, so this is the Workbench's own fact, keyed by the pool's id. A pool without a ceiling has no document.
/// </summary>
/// <param name="Id">The pool's id.</param>
/// <param name="TokensPerDay">The ceiling.</param>
[ReadModel]
[RemovedWith<PoolCeilingCleared>]
public record PoolCeiling(
    string Id,
    [SetFrom<PoolCeilingSet>(nameof(PoolCeilingSet.TokensPerDay))]
    TokenAmount TokensPerDay)
{
    /// <summary>
    /// Observes the ceiling of every pool that has had one.
    /// </summary>
    /// <param name="collection">The projected ceilings.</param>
    /// <returns>The ceilings.</returns>
    public static ISubject<IEnumerable<PoolCeiling>> AllPoolCeilings(IMongoCollection<PoolCeiling> collection) =>
        collection.Observe();
}
