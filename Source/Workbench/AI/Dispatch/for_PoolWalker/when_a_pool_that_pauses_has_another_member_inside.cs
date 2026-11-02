// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolWalker;

public class when_a_pool_that_pauses_has_another_member_inside : Specification
{
    readonly List<string> _tried = [];
    string _served;

    async Task Because() => _served = await PoolWalker.Walk(pools.Plan(PoolFallback.PauseAndAsk, 0, 0, pools.Inside("Local A"), pools.Inside("Local B"), pools.Outside("Cloud")), pools.Attempts(_tried, "Local A"), CancellationToken.None);

    [Fact] void should_move_on_to_the_other_member_inside() => _served.ShouldEqual("Local B");
    [Fact] void should_not_touch_the_one_outside() => _tried.SequenceEqual(["Local A", "Local B"]).ShouldBeTrue();
}
#endif
