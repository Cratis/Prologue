// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Providers;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolOrder;

public class when_ordering_the_members : Specification
{
    readonly AIProviderId _a = new(Guid.NewGuid());
    readonly AIProviderId _b = new(Guid.NewGuid());
    readonly AIProviderId _c = new(Guid.NewGuid());
    readonly AIProviderId _gone = new(Guid.NewGuid());
    IReadOnlyList<AIProviderId> _byPriority;
    IReadOnlyList<AIProviderId> _byCapacity;
    IReadOnlyList<AIProviderId> _byCapacityWhenAllAreEqual;

    double Headroom(AIProviderId id)
    {
        if (id == _a)
        {
            return 0.2;
        }

        return id == _b ? 0.9 : 0.5;
    }

    void Because()
    {
        _byPriority = PoolOrder.Of([_a, _b, _c], [_c, _gone, _b, _c], PoolOrdering.InPriorityOrder, _ => 0);
        _byCapacity = PoolOrder.Of([_a, _b, _c], [_c, _b, _a], PoolOrdering.ByRemainingCapacity, Headroom);
        _byCapacityWhenAllAreEqual = PoolOrder.Of([_a, _b, _c], [_c, _b, _a], PoolOrdering.ByRemainingCapacity, _ => 1);
    }

    [Fact] void should_follow_the_recorded_order_in_priority_order() => _byPriority.Take(2).SequenceEqual([_c, _b]).ShouldBeTrue();
    [Fact] void should_append_a_member_the_order_does_not_mention() => _byPriority[2].ShouldEqual(_a);
    [Fact] void should_drop_a_recorded_member_that_left_the_pool() => _byPriority.Count.ShouldEqual(3);
    [Fact] void should_put_the_most_capacity_first() => _byCapacity.SequenceEqual([_b, _c, _a]).ShouldBeTrue();
    [Fact] void should_keep_the_pools_own_order_when_capacity_is_equal_or_unknown() => _byCapacityWhenAllAreEqual.SequenceEqual([_a, _b, _c]).ShouldBeTrue();
}
#endif
