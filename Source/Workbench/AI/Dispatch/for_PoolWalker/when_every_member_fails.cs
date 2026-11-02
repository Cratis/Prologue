// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolWalker;

public class when_every_member_fails : Specification
{
    Exception _exception;
    Exception _empty;

    async Task Because()
    {
        _exception = await Cratis.Specifications.Catch.Exception(() => PoolWalker.Walk(pools.Plan(PoolFallback.ContinueOnNext, 0, 0, pools.Inside("Local"), pools.Outside("Cloud")), pools.Attempts([], "Local", "Cloud"), CancellationToken.None));
        _empty = await Cratis.Specifications.Catch.Exception(() => PoolWalker.Walk(pools.Plan(PoolFallback.ContinueOnNext, 0, 0), pools.Attempts([]), CancellationToken.None));
    }

    [Fact] void should_say_the_pool_is_exhausted() => _exception.ShouldBeOfExactType<PoolExhausted>();
    [Fact] void should_say_why_each_member_failed() => ((PoolExhausted)_exception).Reasons.SequenceEqual(["Local: Local is down", "Cloud: Cloud is down"]).ShouldBeTrue();
    [Fact] void should_say_an_empty_pool_has_no_providers() => _empty.Message.ShouldContain("has no providers");
}
#endif
