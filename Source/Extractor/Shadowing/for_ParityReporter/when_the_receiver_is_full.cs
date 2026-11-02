// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Prologue.Extractor.Shadowing.for_ParityReporter;

public class when_the_receiver_is_full : Specification
{
    Exception? _escaped;
    int _requests;

    async Task Because()
    {
        var handler = new Unavailable(() => _requests++);
        var observation = new ParityObservation(Guid.NewGuid(), DateTimeOffset.UtcNow, "POST", "/api/loans", "POST /api/loans", "trace", 200, 200, ParityOutcome.Matched);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://receiver.test/") };
        _escaped = await Record.ExceptionAsync(() => new ParityReporter(client, Guid.NewGuid(), NullLogger<ParityReporter>.Instance).Report(observation, CancellationToken.None));
    }

    [Fact] void should_have_posted_the_observation() => _requests.ShouldEqual(1);
    [Fact] void should_treat_the_503_as_a_failed_report_and_not_throw() => _escaped.ShouldBeNull();

    sealed class Unavailable(Action onRequest) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            onRequest();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        }
    }
}
#endif
