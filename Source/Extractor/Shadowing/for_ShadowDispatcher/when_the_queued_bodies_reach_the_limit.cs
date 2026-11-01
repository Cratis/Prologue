// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Shadowing.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Prologue.Extractor.Shadowing.for_ShadowDispatcher;

public class when_the_queued_bodies_reach_the_limit : Specification
{
    readonly List<ParityObservation> _reported = [];

    ShadowDispatcher _dispatcher;
    long _queuedAfterTheFirst;
    bool _secondQueued;
    long _queuedAfterTheSecond;
    IShadowSender _sender;
    long _queuedWhenDone;

    static ShadowJob Job(int bytes) =>
        new(Guid.NewGuid(), Rules.For("POST /api/loans"), DateTimeOffset.UtcNow, "POST", "/api/loans", string.Empty, "POST /api/loans", string.Empty, [new("Content-Type", ["application/json"])], new byte[bytes], 201, false);

    async Task Because()
    {
        _sender = Substitute.For<IShadowSender>();
        _sender.Send(Arg.Any<ShadowJob>(), Arg.Any<CancellationToken>()).Returns(201);
        var reporter = Substitute.For<IParityReporter>();
        reporter.Report(Arg.Do<ParityObservation>(_reported.Add), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _dispatcher = new ShadowDispatcher(_sender, reporter, NullLogger<ShadowDispatcher>.Instance, 100);

        _dispatcher.TryEnqueue(Job(80));
        _queuedAfterTheFirst = _dispatcher.QueuedBytes;
        _secondQueued = _dispatcher.TryEnqueue(Job(80));
        _queuedAfterTheSecond = _dispatcher.QueuedBytes;

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await _dispatcher.StartAsync(cancellation.Token);
        while (_reported.Count < 2 && !cancellation.IsCancellationRequested)
        {
            await Task.Delay(10);
        }

        await _dispatcher.StopAsync(CancellationToken.None);
        _queuedWhenDone = _dispatcher.QueuedBytes;
    }

    [Fact] void should_count_the_body_it_holds() => _queuedAfterTheFirst.ShouldEqual(80);
    [Fact] void should_still_take_the_request_as_a_skipped_one() => _secondQueued.ShouldBeTrue();
    [Fact] void should_not_count_the_body_it_did_not_keep() => _queuedAfterTheSecond.ShouldEqual(80);
    [Fact] void should_report_the_request_past_the_limit_as_skipped() => _reported.Count(observation => observation.Outcome == ParityOutcome.Skipped).ShouldEqual(1);
    [Fact] void should_mirror_the_other_one() => _sender.Received(1).Send(Arg.Any<ShadowJob>(), Arg.Any<CancellationToken>());
    [Fact] void should_release_the_bytes_when_the_work_is_done() => _queuedWhenDone.ShouldEqual(0);
}
#endif
