// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Rebuild.Parity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cratis.Prologue.Workbench.Rebuild.for_ParityRecording;

public class when_the_queue_is_full_the_endpoint_answers : Specification
{
    IResult _queued;
    IResult _full;

    static ParityObservation Observation() => new(Guid.NewGuid(), DateTimeOffset.UtcNow, "POST", "/api/loans", "POST /api/loans", "trace", 200, 200, ParityOutcome.Matched);

    void Because()
    {
        var queue = new ParityRecordingQueue(Microsoft.Extensions.Logging.Abstractions.NullLogger<ParityRecordingQueue>.Instance, 1);
        _queued = ParityEndpoint.Accept(Guid.NewGuid(), Observation(), queue);
        _full = ParityEndpoint.Accept(Guid.NewGuid(), Observation(), queue);
    }

    [Fact] void should_accept_what_fits() => _queued.ShouldBeOfExactType<Accepted>();
    [Fact] void should_answer_503_when_it_does_not_fit() => ((StatusCodeHttpResult)_full).StatusCode.ShouldEqual(StatusCodes.Status503ServiceUnavailable);
}
#endif
