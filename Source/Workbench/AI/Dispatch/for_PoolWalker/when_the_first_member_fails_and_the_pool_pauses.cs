// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolWalker;

public class when_the_first_member_fails_and_the_pool_pauses : Specification
{
    readonly List<string> _tried = [];
    Exception _exception;

    async Task Because() => _exception = await Cratis.Specifications.Catch.Exception(() => PoolWalker.Walk(pools.Plan(PoolFallback.PauseAndAsk, 0, 0, pools.Inside("Local"), pools.Outside("Cloud")), pools.Attempts(_tried, "Local"), CancellationToken.None));

    [Fact] void should_pause() => _exception.ShouldBeOfExactType<AgentDispatchPaused>();
    [Fact] void should_name_the_provider_that_would_be_next() => ((AgentDispatchPaused)_exception).Next.ShouldEqual("Cloud");
    [Fact] void should_name_the_provider_that_could_not_take_it() => ((AgentDispatchPaused)_exception).Failed.ShouldEqual("Local");
    [Fact] void should_say_what_to_do_in_the_message() => _exception.Message.ShouldContain("Systems page");
    [Fact] void should_report_the_pause_as_the_outcome() => ((AgentDispatchStopped)_exception).Outcome.ShouldEqual(AgentDispatchOutcome.Paused);
    [Fact] void should_never_reach_the_provider_outside() => _tried.SequenceEqual(["Local"]).ShouldBeTrue();
}
#endif
