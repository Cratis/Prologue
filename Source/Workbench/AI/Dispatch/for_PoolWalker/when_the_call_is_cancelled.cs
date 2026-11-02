// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolWalker;

public class when_the_call_is_cancelled : Specification
{
    Exception _exception;

    async Task Because() => _exception = await Cratis.Specifications.Catch.Exception(() => PoolWalker.Walk<string>(pools.Plan(PoolFallback.ContinueOnNext, 0, 0, pools.Inside("Local"), pools.Outside("Cloud")), _ => throw new OperationCanceledException(), CancellationToken.None));

    [Fact] void should_not_move_on_to_the_next_member() => _exception.ShouldBeOfExactType<OperationCanceledException>();
}
#endif
