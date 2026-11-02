// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Conversations;
using Cratis.AI.LanguageModels;
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_DispatchingChatClient;

public class when_calls_arrive_at_the_same_time : Specification
{
    const int Calls = 20;
    int _reachedTheProvider;
    int _stopped;
    Exception _afterwards;

    async Task Because()
    {
        var purpose = new LanguageModelPurpose("prologue-dramaturg");
        var member = pools.Inside("Local");
        var inner = Substitute.For<IAIChatClients>();
        inner.For(Arg.Any<LanguageModelPurpose>(), Arg.Any<ChatAgentTarget>()).Returns(_ =>
        {
            Interlocked.Increment(ref _reachedTheProvider);
            return pools.Answering(PoolSpendLedger.DefaultEstimate, 50);
        });
        var ledger = new PoolSpendLedger(Substitute.For<IPoolSpendRecorder>(), TimeProvider.System, Microsoft.Extensions.Logging.Abstractions.NullLogger<PoolSpendLedger>.Instance);
        var plans = Substitute.For<IPoolPlans>();
        var key = Guid.NewGuid().ToString();
        plans.For(purpose, Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult<PoolPlan?>(pools.Plan(PoolFallback.ContinueOnNext, PoolSpendLedger.DefaultEstimate * 5, 0, member) with { PoolKey = key }));
        var client = new DispatchingChatClient(purpose, inner, plans, ledger);

        var outcomes = await Task.WhenAll(Enumerable.Range(0, Calls).Select(_ => Task.Run(async () => await Cratis.Specifications.Catch.Exception(() => client.GetResponseAsync(pools.Hello, null, TestContext.Current.CancellationToken)))));
        _stopped = outcomes.Count(outcome => outcome is PoolCeilingReached);
        _afterwards = await Cratis.Specifications.Catch.Exception(() => client.GetResponseAsync(pools.Hello, null, TestContext.Current.CancellationToken));
    }

    [Fact] void should_let_through_no_more_calls_than_the_ceiling_has_room_for() => _reachedTheProvider.ShouldEqual(5);
    [Fact] void should_stop_every_other_call_at_the_ceiling() => _stopped.ShouldEqual(Calls - 5);
    [Fact] void should_still_be_at_the_ceiling_when_the_answers_are_in() => _afterwards.ShouldBeOfExactType<PoolCeilingReached>();
}
#endif
