// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolWalker;

public class when_the_ceiling_is_reached : Specification
{
    readonly List<string> _tried = [];
    Exception _exception;
    string _belowTheCeiling;
    string _withoutACeiling;

    async Task Because()
    {
        _exception = await Cratis.Specifications.Catch.Exception(() => PoolWalker.Walk(pools.Plan(PoolFallback.ContinueOnNext, 1000, 1000, pools.Inside("Local")), pools.Attempts(_tried), CancellationToken.None));
        _belowTheCeiling = await PoolWalker.Walk(pools.Plan(PoolFallback.ContinueOnNext, 1000, 999, pools.Inside("Local")), pools.Attempts(_tried), CancellationToken.None);
        _withoutACeiling = await PoolWalker.Walk(pools.Plan(PoolFallback.ContinueOnNext, 0, 5_000_000, pools.Inside("Local")), pools.Attempts(_tried), CancellationToken.None);
    }

    [Fact] void should_stop_with_the_ceiling_outcome() => ((AgentDispatchStopped)_exception).Outcome.ShouldEqual(AgentDispatchOutcome.CeilingReached);
    [Fact] void should_state_the_ceiling_and_the_use() => _exception.Message.ShouldContain("1,000 tokens");
    [Fact] void should_call_no_provider_when_stopped() => _tried.Count.ShouldEqual(2);
    [Fact] void should_serve_below_the_ceiling() => _belowTheCeiling.ShouldEqual("Local");
    [Fact] void should_not_limit_a_pool_without_a_ceiling() => _withoutACeiling.ShouldEqual("Local");
}
#endif
