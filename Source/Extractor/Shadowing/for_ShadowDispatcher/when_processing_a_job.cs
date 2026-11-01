// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Shadowing.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Prologue.Extractor.Shadowing.for_ShadowDispatcher;

public class when_processing_a_job : Specification
{
    IShadowSender _sender;
    IParityReporter _reporter;
    ShadowDispatcher _dispatcher;
    List<ParityObservation> _reported;

    void Establish()
    {
        _sender = Substitute.For<IShadowSender>();
        _reporter = Substitute.For<IParityReporter>();
        _reported = [];
        _reporter.When(reporter => reporter.Report(Arg.Any<ParityObservation>(), Arg.Any<CancellationToken>())).Do(call => _reported.Add(call.Arg<ParityObservation>()));
        _dispatcher = new ShadowDispatcher(_sender, _reporter, NullLogger<ShadowDispatcher>.Instance, ShadowLimits.DefaultQueuedBytes);
    }

    async Task Because()
    {
        _sender.Send(Arg.Any<ShadowJob>(), Arg.Any<CancellationToken>()).Returns(201, 409, null);
        await _dispatcher.Process(Job(201), CancellationToken.None);
        await _dispatcher.Process(Job(201), CancellationToken.None);
        await _dispatcher.Process(Job(201), CancellationToken.None);
        await _dispatcher.Process(Job(201, skipped: true), CancellationToken.None);
    }

    static ShadowJob Job(int legacy, bool skipped = false) =>
        new(Guid.NewGuid(), Rules.For("POST /api/loans"), DateTimeOffset.UtcNow, "POST", "/api/loans/4412", string.Empty, "POST /api/loans/{id}", "trace", [], skipped ? null : [1], legacy, skipped);

    [Fact] void should_report_every_request() => _reported.Count.ShouldEqual(4);
    [Fact] void should_report_a_match() => _reported[0].Outcome.ShouldEqual(ParityOutcome.Matched);
    [Fact] void should_report_a_mismatch_with_both_statuses() => (_reported[1].LegacyStatus, _reported[1].ShadowStatus, _reported[1].Outcome).ShouldEqual((201, 409, ParityOutcome.Mismatched));
    [Fact] void should_report_an_unreachable_shadow_as_unavailable_and_not_as_a_mismatch() => (_reported[2].Outcome, _reported[2].ShadowStatus).ShouldEqual((ParityOutcome.Unavailable, 0));
    [Fact] void should_report_a_skipped_body_without_sending_it() => (_reported[3].Outcome, _sender.ReceivedCalls().Count()).ShouldEqual((ParityOutcome.Skipped, 3));
    [Fact] void should_report_the_shape_path_and_trace() => (_reported[0].Shape, _reported[0].Path, _reported[0].Trace).ShouldEqual(("POST /api/loans/{id}", "/api/loans/4412", "trace"));
}
#endif
