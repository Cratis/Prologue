// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Conversations;
using Cratis.AI.LanguageModels;
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_DispatchingChatClient;

public class when_the_pool_reached_its_ceiling : Specification
{
    IAIChatClients _inner;
    Exception _exception;

    async Task Because()
    {
        var purpose = new LanguageModelPurpose("prologue-dramaturg");
        _inner = Substitute.For<IAIChatClients>();
        var plans = Substitute.For<IPoolPlans>();
        plans.For(purpose, Arg.Any<CancellationToken>()).Returns(Task.FromResult<PoolPlan?>(pools.Plan(PoolFallback.ContinueOnNext, 100, 120, pools.Inside("Local"))));
        _exception = await Cratis.Specifications.Catch.Exception(() => new DispatchingChatClient(purpose, _inner, plans, new PoolSpendLedger(Substitute.For<IPoolSpendRecorder>(), TimeProvider.System, Microsoft.Extensions.Logging.Abstractions.NullLogger<PoolSpendLedger>.Instance)).GetResponseAsync(pools.Hello, null, TestContext.Current.CancellationToken));
    }

    [Fact] void should_fail_with_the_ceiling_outcome() => _exception.ShouldBeOfExactType<PoolCeilingReached>();
    [Fact] void should_not_ask_cratis_ai_for_any_provider() => _inner.DidNotReceiveWithAnyArgs().For(default!, default!);
}
#endif
