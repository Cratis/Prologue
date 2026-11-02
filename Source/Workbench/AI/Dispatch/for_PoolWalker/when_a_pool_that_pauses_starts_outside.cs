// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolWalker;

// The pool "leaves the network" for the posture, because it does not start inside; dispatch reaches the provider outside straight away.
public class when_a_pool_that_pauses_starts_outside : Specification
{
    readonly List<string> _tried = [];
    string _served;

    async Task Because() => _served = await PoolWalker.Walk(pools.Plan(PoolFallback.PauseAndAsk, 0, 0, pools.Outside("Cloud"), pools.Inside("Local")), pools.Attempts(_tried), CancellationToken.None);

    [Fact] void should_use_the_first_member_even_though_it_is_outside() => _served.ShouldEqual("Cloud");
}
#endif
