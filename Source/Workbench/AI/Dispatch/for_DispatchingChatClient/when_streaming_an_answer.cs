// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Runtime.CompilerServices;
using Cratis.AI.Conversations;
using Cratis.AI.LanguageModels;
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_DispatchingChatClient;

public class when_streaming_an_answer : Specification
{
    IPoolSpendRecorder _completed;
    IPoolSpendRecorder _stoppedEarly;
    IPoolSpendRecorder _failed;
    Exception _failure;

    static ChatResponseUpdate Update(string text, long? tokens = null) =>
        new(ChatRole.Assistant, tokens is null ? [new TextContent(text)] : [new TextContent(text), new UsageContent(new UsageDetails { TotalTokenCount = tokens })]);

    static async IAsyncEnumerable<ChatResponseUpdate> Stream(bool failAtTheEnd, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        yield return Update("a", 10);
        yield return Update("b");
        yield return Update("c", 5);
        if (failAtTheEnd)
        {
            throw new InvalidOperationException("The connection dropped");
        }
    }

    static (DispatchingChatClient Client, PoolSpendLedger Ledger, IPoolSpendRecorder Recorder) Setup(bool failAtTheEnd)
    {
        var purpose = new LanguageModelPurpose("prologue-dramaturg");
        var vendor = Substitute.For<IChatClient>();
        vendor.GetStreamingResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>()).Returns(_ => Stream(failAtTheEnd));
        var inner = Substitute.For<IAIChatClients>();
        inner.For(Arg.Any<LanguageModelPurpose>(), Arg.Any<ChatAgentTarget>()).Returns(vendor);
        var recorder = Substitute.For<IPoolSpendRecorder>();
        recorder.Record(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(true);
        var ledger = new PoolSpendLedger(recorder, TimeProvider.System, Microsoft.Extensions.Logging.Abstractions.NullLogger<PoolSpendLedger>.Instance);
        var plans = Substitute.For<IPoolPlans>();
        plans.For(purpose, Arg.Any<CancellationToken>()).Returns(Task.FromResult<PoolPlan?>(pools.Plan(PoolFallback.ContinueOnNext, 0, 0, pools.Inside("Local"))));
        return (new DispatchingChatClient(purpose, inner, plans, ledger), ledger, recorder);
    }

    async Task Because()
    {
        var (client, ledger, recorder) = Setup(false);
        await foreach (var update in client.GetStreamingResponseAsync(pools.Hello, null, TestContext.Current.CancellationToken))
        {
            _ = update;
        }

        await ledger.FlushAll(TestContext.Current.CancellationToken);
        _completed = recorder;

        (client, ledger, recorder) = Setup(false);
        await foreach (var update in client.GetStreamingResponseAsync(pools.Hello, null, TestContext.Current.CancellationToken))
        {
            _ = update;
            break;
        }

        await ledger.FlushAll(TestContext.Current.CancellationToken);
        _stoppedEarly = recorder;

        (client, ledger, recorder) = Setup(true);
        _failure = await Cratis.Specifications.Catch.Exception(async () =>
        {
            await foreach (var update in client.GetStreamingResponseAsync(pools.Hello, null, TestContext.Current.CancellationToken))
            {
                _ = update;
            }
        });
        await ledger.FlushAll(TestContext.Current.CancellationToken);
        _failed = recorder;
    }

    [Fact] void should_sum_every_usage_update() => _completed.Received(1).Record(Arg.Any<string>(), Arg.Any<string>(), 15, Arg.Any<CancellationToken>());
    [Fact] void should_record_what_was_used_when_the_consumer_stops_early() => _stoppedEarly.Received(1).Record(Arg.Any<string>(), Arg.Any<string>(), 10, Arg.Any<CancellationToken>());
    [Fact] void should_fail_the_stream_that_broke() => _failure.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_record_what_a_broken_stream_used_exactly_once() => _failed.Received(1).Record(Arg.Any<string>(), Arg.Any<string>(), 15, Arg.Any<CancellationToken>());
}
#endif
