// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Providers;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Decides the order a pool tries its members in.
/// </summary>
public static class PoolOrder
{
    /// <summary>
    /// Orders the members. In priority order the recorded order comes first, for the members still in the pool, followed by any member
    /// it does not mention in the order the pool lists them. By remaining capacity the pool's own order is kept as the tie-break
    /// and the member with the most headroom goes first; Cratis.AI reports full headroom for a provider it cannot measure, so
    /// that provider keeps its place rather than being sent to the back.
    /// </summary>
    /// <param name="listed">The members as the pool lists them.</param>
    /// <param name="recorded">The order a person recorded for the pool.</param>
    /// <param name="ordering">The <see cref="PoolOrdering"/>.</param>
    /// <param name="headroom">Answers how much of a member's allowance is left, from 0 to 1.</param>
    /// <returns>Every member once, first choice first.</returns>
    public static IReadOnlyList<AIProviderId> Of(IReadOnlyList<AIProviderId> listed, IEnumerable<AIProviderId> recorded, PoolOrdering ordering, Func<AIProviderId, double> headroom)
    {
        if (ordering == PoolOrdering.ByRemainingCapacity)
        {
            return [.. listed.Select((member, index) => (member, index)).OrderByDescending(_ => headroom(_.member)).ThenBy(_ => _.index).Select(_ => _.member)];
        }

        var chosen = recorded.Where(listed.Contains).Distinct().ToList();
        return [.. chosen, .. listed.Where(member => !chosen.Contains(member))];
    }
}
