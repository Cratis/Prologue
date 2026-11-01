// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Prologue.Configuration;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Yarp.ReverseProxy.Forwarder;

namespace Cratis.Prologue.Extractor.Shadowing.for_ShadowMiddleware;

public class when_a_matching_request_passes_through : Specification
{
    List<ShadowJob> _queued;
    bool _answeredBeforeQueueing;
    string _bodySeenByLegacy;
    ShadowJob _job;
    int _passedThrough;
    int _skippedForSize;
    int _failedQueue;
    int _noLegacyAnswer;
    string _responseAfterFailure;

    ShadowMiddleware Middleware(RequestDelegate next, IShadowQueue? queue = null) => new(
        next,
        new ShadowRouting([new ShadowRuleOptions { Shape = "POST /api/loans/{id}/return", Destination = "http://new-system:5000/" }]),
        queue ?? new Queue(_queued),
        NullLogger<ShadowMiddleware>.Instance);

    DefaultHttpContext Request(string method, string path, string body, bool setLength = true)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.QueryString = new("?force=true");
        context.Request.Headers.Authorization = "Bearer value";
        context.Request.Headers["X-Correlation"] = "abc";
        context.Request.Headers.TraceParent = "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        if (setLength)
        {
            context.Request.ContentLength = Encoding.UTF8.GetByteCount(body);
        }

        context.Response.Body = new MemoryStream();
        return context;
    }

    static Task Accept(HttpContext http)
    {
        http.Response.StatusCode = 201;
        return Task.CompletedTask;
    }

    static Task NoContent(HttpContext http)
    {
        http.Response.StatusCode = 204;
        return Task.CompletedTask;
    }

    Task CountPassThrough(HttpContext http)
    {
        _passedThrough++;
        return Task.CompletedTask;
    }

    async Task Because()
    {
        _queued = [];

        var context = Request("POST", "/api/loans/4412/return", "{\"returned\":true}");
        await Middleware(async http =>
        {
            _bodySeenByLegacy = await new StreamReader(http.Request.Body).ReadToEndAsync();
            http.Response.StatusCode = 201;
            _answeredBeforeQueueing = _queued.Count == 0;
        }).InvokeAsync(context);
        _job = _queued.Single();

        await Middleware(CountPassThrough).InvokeAsync(Request("GET", "/api/loans/4412/return", string.Empty));
        await Middleware(CountPassThrough).InvokeAsync(Request("POST", "/api/members", "{}"));

        _queued.Clear();
        var tooLarge = Request("POST", "/api/loans/4412/return", "x");
        tooLarge.Request.ContentLength = ShadowLimits.MaximumBodyBytes + 1;
        await Middleware(Accept).InvokeAsync(tooLarge);
        _skippedForSize = _queued.Count(job => job.BodySkipped && job.Body is null && job.Headers.Count == 0);

        _queued.Clear();
        var unreachable = Request("POST", "/api/loans/4412/return", "{}");
        await Middleware(http =>
        {
            http.Features.Set<IForwarderErrorFeature>(new ErrorFeature());
            http.Response.StatusCode = 502;
            return Task.CompletedTask;
        }).InvokeAsync(unreachable);
        _noLegacyAnswer = _queued.Count;

        var failing = Request("POST", "/api/loans/4412/return", "{}");
        await Middleware(NoContent, new ThrowingQueue()).InvokeAsync(failing);
        _responseAfterFailure = failing.Response.StatusCode.ToString();
        _failedQueue = _queued.Count;
    }

    [Fact] void should_let_the_legacy_system_read_the_whole_body() => _bodySeenByLegacy.ShouldEqual("{\"returned\":true}");
    [Fact] void should_queue_the_copy_only_after_the_legacy_system_answered() => _answeredBeforeQueueing.ShouldBeTrue();
    [Fact] void should_copy_the_body_the_legacy_system_already_consumed() => Encoding.UTF8.GetString(_job.Body!).ShouldEqual("{\"returned\":true}");
    [Fact] void should_carry_the_legacy_status() => _job.LegacyStatus.ShouldEqual(201);
    [Fact] void should_carry_the_shape_and_the_path_with_its_identity() => (_job.Shape, _job.Path, _job.Query).ShouldEqual(("POST /api/loans/{id}/return", "/api/loans/4412/return", "?force=true"));
    [Fact] void should_carry_the_trace() => _job.Trace.ShouldEqual("0af7651916cd43dd8448eb211c80319c");
    [Fact] void should_leave_credentials_out_of_the_headers() => _job.Headers.Any(header => header.Key == "Authorization").ShouldBeFalse();
    [Fact] void should_keep_the_other_headers() => _job.Headers.Any(header => header.Key == "X-Correlation").ShouldBeTrue();
    [Fact] void should_pass_other_requests_through_untouched() => _passedThrough.ShouldEqual(2);
    [Fact] void should_report_a_body_over_the_limit_as_skipped_instead_of_copying_it() => _skippedForSize.ShouldEqual(1);
    [Fact] void should_not_mirror_when_the_legacy_system_never_answered() => _noLegacyAnswer.ShouldEqual(0);
    [Fact] void should_leave_the_response_alone_when_queueing_fails() => _responseAfterFailure.ShouldEqual("204");

    sealed class Queue(List<ShadowJob> jobs) : IShadowQueue
    {
        public bool TryEnqueue(ShadowJob job)
        {
            jobs.Add(job);
            return true;
        }
    }

    sealed class ThrowingQueue : IShadowQueue
    {
        public bool TryEnqueue(ShadowJob job) => throw new InvalidOperationException("boom");
    }

    sealed class ErrorFeature : IForwarderErrorFeature
    {
        public ForwarderError Error => ForwarderError.Request;

        public Exception? Exception => null;
    }
}
#endif
