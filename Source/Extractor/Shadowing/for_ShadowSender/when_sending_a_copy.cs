// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Prologue.Extractor.Shadowing.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Prologue.Extractor.Shadowing.for_ShadowSender;

public class when_sending_a_copy : Specification
{
    RecordingHandler _handler;
    int? _status;
    int? _unreachable;
    Exception? _cancellation;

    void Establish() => _handler = new RecordingHandler(HttpStatusCode.Created);

    async Task Because()
    {
        var rule = Rules.For("POST /api/loans");
        var headers = ShadowHeaders.Select([new("Content-Type", "application/json"), new("X-Correlation", "abc"), new("Authorization", "Bearer value")], rule.ForwardHeaders);
        var job = new ShadowJob(Guid.NewGuid(), rule, DateTimeOffset.UtcNow, "POST", "/api/loans", "?dryRun=false", "POST /api/loans", string.Empty, headers, "{\"book\":1}"u8.ToArray(), 201, false);

        _status = await new ShadowSender(new HttpClient(_handler), NullLogger<ShadowSender>.Instance).Send(job, CancellationToken.None);
        _unreachable = await new ShadowSender(new HttpClient(new RecordingHandler(null)), NullLogger<ShadowSender>.Instance).Send(job, CancellationToken.None);
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        _cancellation = await Record.ExceptionAsync(() => new ShadowSender(new HttpClient(new RecordingHandler(null)), NullLogger<ShadowSender>.Instance).Send(job, cancelled.Token));
    }

    [Fact] void should_report_the_status_the_destination_answered_with() => _status.ShouldEqual(201);
    [Fact] void should_send_the_same_method() => _handler.Method.ShouldEqual(HttpMethod.Post);
    [Fact] void should_send_to_the_destination_with_the_same_path_and_query() => _handler.Uri.ShouldEqual("http://new-system:5000/api/loans?dryRun=false");
    [Fact] void should_send_the_body() => _handler.Body.ShouldEqual("{\"book\":1}");
    [Fact] void should_send_the_content_type() => _handler.ContentType.ShouldEqual("application/json");
    [Fact] void should_send_the_headers_it_was_given() => _handler.Headers["X-Correlation"].ShouldEqual("abc");
    [Fact] void should_not_send_a_credential_that_was_dropped() => _handler.Headers.ContainsKey("Authorization").ShouldBeFalse();
    [Fact] void should_mark_the_request_as_a_copy() => _handler.Headers[ShadowHeaders.MarkerHeader].ShouldEqual("true");
    [Fact] void should_report_nothing_when_the_destination_cannot_be_reached() => _unreachable.ShouldBeNull();
    [Fact] void should_let_a_shutdown_cancel_it() => (_cancellation is OperationCanceledException).ShouldBeTrue();
}

class RecordingHandler(HttpStatusCode? status) : HttpMessageHandler
{
    public HttpMethod? Method { get; private set; }

    public string Uri { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (status is null)
        {
            throw new HttpRequestException("unreachable");
        }

        Method = request.Method;
        Uri = request.RequestUri!.ToString();
        Body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        ContentType = request.Content?.Headers.ContentType?.ToString() ?? string.Empty;
        foreach (var header in request.Headers)
        {
            Headers[header.Key] = string.Join(',', header.Value);
        }

        return new HttpResponseMessage(status.Value);
    }
}
#endif
