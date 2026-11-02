// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Conversations;
using Cratis.AI.Providers;
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_DispatchingChatClient;

public class when_the_local_member_fails : Specification
{
    IAIChatClients _inner;
    IPoolSpendRecorder _recorder;
    PoolSpendLedger _spend;
    DispatchMember _local;
    DispatchMember _cloud;
    List<AIProviderId> _called;
    Exception _paused;
    ChatResponse _continued;

    static IChatClient Client(Func<ChatResponse> respond)
    {
        var client = Substitute.For<IChatClient>();
        client.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult(respond()));
        return client;
    }

    void Establish()
    {
        _local = pools.Inside("Local");
        _cloud = pools.Outside("Cloud");
        _called = [];
        _recorder = Substitute.For<IPoolSpendRecorder>();
        _recorder.Record(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(true);
        _spend = new PoolSpendLedger(_recorder, TimeProvider.System, Microsoft.Extensions.Logging.Abstractions.NullLogger<PoolSpendLedger>.Instance);
        _inner = Substitute.For<IAIChatClients>();
        _inner.For(Arg.Any<Cratis.AI.LanguageModels.LanguageModelPurpose>(), Arg.Any<ChatAgentTarget>()).Returns(call =>
        {
            var provider = call.Arg<ChatAgentTarget>().ProviderId!;
            _called.Add(provider);
            return provider == _local.Provider
                ? Client(() => throw new InvalidOperationException("The local model is down"))
                : Client(() => new ChatResponse(new ChatMessage(ChatRole.Assistant, "from the cloud")) { Usage = new UsageDetails { TotalTokenCount = 42 } });
        });
    }

    DispatchingChatClient ClientFor(PoolFallback fallback)
    {
        var plans = Substitute.For<IPoolPlans>();
        plans.For(Arg.Any<Cratis.AI.LanguageModels.LanguageModelPurpose>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<PoolPlan?>(pools.Plan(fallback, 0, 0, _local, _cloud)));
        return new(new Cratis.AI.LanguageModels.LanguageModelPurpose("prologue-dramaturg"), _inner, plans, _spend);
    }

    async Task Because()
    {
        _paused = await Cratis.Specifications.Catch.Exception(() => ClientFor(PoolFallback.PauseAndAsk).GetResponseAsync(pools.Hello, null, TestContext.Current.CancellationToken));
        var calledWhilePaused = _called.ToList();
        _called.Clear();
        _continued = await ClientFor(PoolFallback.ContinueOnNext).GetResponseAsync(pools.Hello, null, TestContext.Current.CancellationToken);
        _calledWhilePaused = calledWhilePaused;
        await _spend.FlushAll(TestContext.Current.CancellationToken);
    }

    List<AIProviderId> _calledWhilePaused;

    [Fact] void should_pause_instead_of_reaching_the_provider_outside() => _paused.ShouldBeOfExactType<AgentDispatchPaused>();
    [Fact] void should_only_have_called_the_local_provider_while_paused() => _calledWhilePaused.SequenceEqual([_local.Provider]).ShouldBeTrue();
    [Fact] void should_continue_to_the_provider_outside_when_the_pool_continues() => _continued.Text.ShouldEqual("from the cloud");
    [Fact] void should_call_the_providers_in_order_when_continuing() => _called.SequenceEqual([_local.Provider, _cloud.Provider]).ShouldBeTrue();
    [Fact] void should_record_the_tokens_the_answer_used_against_the_pool() => _recorder.Received(1).Record(Arg.Any<string>(), Arg.Any<string>(), 42, Arg.Any<CancellationToken>());
}
#endif
