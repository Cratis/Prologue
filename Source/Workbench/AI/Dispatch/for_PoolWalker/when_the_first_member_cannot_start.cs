// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolWalker;

public class when_the_first_member_cannot_start : Specification
{
    readonly List<string> _tried = [];
    string _served;
    Exception _paused;

    async Task Because()
    {
        _served = await PoolWalker.Walk(pools.Plan(PoolFallback.ContinueOnNext, 0, 0, pools.Inside("Local", 0, false), pools.Outside("Cloud")), pools.Attempts(_tried), CancellationToken.None);
        _paused = await Cratis.Specifications.Catch.Exception(() => PoolWalker.Walk(pools.Plan(PoolFallback.PauseAndAsk, 0, 0, pools.Inside("Local", 0, false), pools.Outside("Cloud")), pools.Attempts(_tried), CancellationToken.None));
    }

    [Fact] void should_continue_to_the_next_member() => _served.ShouldEqual("Cloud");
    [Fact] void should_not_even_call_the_member_that_cannot_start() => _tried.SequenceEqual(["Cloud"]).ShouldBeTrue();
    [Fact] void should_pause_when_the_next_member_is_outside_and_the_pool_pauses() => _paused.ShouldBeOfExactType<AgentDispatchPaused>();
}
#endif
