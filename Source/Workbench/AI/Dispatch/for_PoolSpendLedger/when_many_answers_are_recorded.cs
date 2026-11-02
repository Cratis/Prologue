// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolSpendLedger;

public class when_many_answers_are_recorded : Specification
{
    IPoolSpendRecorder _recorder;
    long _total;
    int _batches;

    async Task Because()
    {
        _recorder = Substitute.For<IPoolSpendRecorder>();
        _recorder.Record(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            Interlocked.Add(ref _total, call.ArgAt<long>(2));
            Interlocked.Increment(ref _batches);
            return Task.FromResult(true);
        });
        var ledger = new PoolSpendLedger(_recorder, new a_clock(), Microsoft.Extensions.Logging.Abstractions.NullLogger<PoolSpendLedger>.Instance);
        var plan = pools.Plan(PoolFallback.ContinueOnNext, 0, 0, pools.Inside("Local"));
        for (var call = 0; call < PoolSpendLedger.MaximumCalls * 2; call++)
        {
            using var reservation = ledger.Reserve(plan);
            reservation.Settle(new UsageDetails { TotalTokenCount = 10 });
        }

        await ledger.FlushAll(TestContext.Current.CancellationToken);
    }

    [Fact] void should_write_every_token() => _total.ShouldEqual(PoolSpendLedger.MaximumCalls * 2 * 10);
    [Fact] void should_write_far_fewer_events_than_answers() => _batches.ShouldBeLessThan(PoolSpendLedger.MaximumCalls * 2);
}
#endif
