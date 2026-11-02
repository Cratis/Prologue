// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Providers;
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolSpendLedger;

public class when_answers_are_recorded : Specification
{
    IPoolSpendRecorder _recorder;
    a_clock _clock;
    PoolSpendLedger _ledger;
    PoolPlan _plan;
    int _recordedBeforeTheWait;
    int _recordedAfterTheWait;
    int _recordedAfterTheFailure;
    long _lastBatch;
    bool _accept;

    void Settle(long tokens)
    {
        using var reservation = _ledger.Reserve(_plan);
        reservation.Settle(new UsageDetails { TotalTokenCount = tokens });
    }

    async Task Because()
    {
        _clock = new a_clock();
        _accept = true;
        _recorder = Substitute.For<IPoolSpendRecorder>();
        _recorder.Record(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            _lastBatch = call.ArgAt<long>(2);
            return Task.FromResult(_accept);
        });
        _ledger = new PoolSpendLedger(_recorder, _clock, Microsoft.Extensions.Logging.Abstractions.NullLogger<PoolSpendLedger>.Instance);
        _plan = pools.Plan(PoolFallback.ContinueOnNext, 0, 0, pools.Inside("Local"));

        Settle(100);
        Settle(200);
        await _ledger.FlushDue(TestContext.Current.CancellationToken);
        _recordedBeforeTheWait = _recorder.ReceivedCalls().Count();

        _clock.Advance(PoolSpendLedger.MaximumWait);
        _accept = false;
        await _ledger.FlushDue(TestContext.Current.CancellationToken);
        _recordedAfterTheFailure = _recorder.ReceivedCalls().Count();

        _accept = true;
        Settle(50);
        await _ledger.FlushDue(TestContext.Current.CancellationToken);
        _recordedAfterTheWait = _recorder.ReceivedCalls().Count();
    }

    [Fact] void should_hold_the_spend_until_it_has_waited_long_enough() => _recordedBeforeTheWait.ShouldEqual(0);
    [Fact] void should_try_to_write_it_when_it_has_waited() => _recordedAfterTheFailure.ShouldEqual(1);
    [Fact] void should_keep_what_could_not_be_written_and_write_it_with_the_next() => _lastBatch.ShouldEqual(350);
    [Fact] void should_write_one_batch_for_all_the_answers() => _recordedAfterTheWait.ShouldEqual(2);
}
#endif
