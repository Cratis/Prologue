// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolWalker;

public class when_the_first_member_fails_and_the_pool_continues : Specification
{
    readonly List<string> _tried = [];
    string _served;

    async Task Because() => _served = await PoolWalker.Walk(pools.Plan(PoolFallback.ContinueOnNext, 0, 0, pools.Inside("Local"), pools.Outside("Cloud")), pools.Attempts(_tried, "Local"), CancellationToken.None);

    [Fact] void should_serve_from_the_next_member() => _served.ShouldEqual("Cloud");
    [Fact] void should_try_the_members_in_order() => _tried.SequenceEqual(["Local", "Cloud"]).ShouldBeTrue();
}
#endif
