// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http.Features;
using Yarp.ReverseProxy.Forwarder;

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Sits in the reverse-proxy pipeline. For a request a shadowing rule matches it lets the legacy system answer exactly as it would
/// have, then hands a copy of the request to the <see cref="IShadowQueue"/>. Nothing it does can change or delay the caller's
/// response: the body is only buffered when its size is known and small, and every failure after the response is logged and dropped.
/// </summary>
/// <param name="next">The next step in the pipeline, the proxy.</param>
/// <param name="routing">Finds the rule that shadows a request.</param>
/// <param name="queue">Takes the copies to mirror.</param>
/// <param name="logger">The logger.</param>
public class ShadowMiddleware(RequestDelegate next, ShadowRouting routing, IShadowQueue queue, ILogger<ShadowMiddleware> logger)
{
    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="context">The context of the request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var rule = routing.Find(context.Request.Method, context.Request.Path);
        if (rule is null)
        {
            await next(context);
            return;
        }

        var plan = Plan(context);
        var occurred = DateTimeOffset.UtcNow;
        await next(context);

        try
        {
            // The caller has everything it will get; flush before doing any work of our own.
            await context.Response.CompleteAsync();
            await Mirror(context, rule, plan, occurred);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            ShadowingLog.ShadowingFailed(logger, rule.Label, exception);
        }
    }

    static BodyPlan Plan(HttpContext context)
    {
        // A body with no Content-Length (chunked, or HTTP/2 without one) cannot be sized up front, so it is never buffered.
        var request = context.Request;
        var sizeUnknown = request.Headers.TransferEncoding.Count > 0 ||
            (request.ContentLength is null && context.Features.Get<IHttpRequestBodyDetectionFeature>()?.CanHaveBody == true);
        var plan = BodyPlans.For(request.ContentLength, sizeUnknown);
        if (plan == BodyPlan.Buffer)
        {
            request.EnableBuffering(bufferThreshold: 30 * 1024, bufferLimit: ShadowLimits.MaximumBodyBytes);
        }

        return plan;
    }

    static async Task<byte[]?> ReadBody(HttpRequest request)
    {
        request.Body.Seek(0, SeekOrigin.Begin);
        await using var copy = new MemoryStream();
        await request.Body.CopyToAsync(copy);
        return copy.ToArray();
    }

    static string TraceOf(HttpRequest request)
    {
        var segments = request.Headers.TraceParent.ToString().Split('-');
        return segments.Length >= 2 && segments[1].Length == 32 ? segments[1] : string.Empty;
    }

    async Task Mirror(HttpContext context, ShadowRule rule, BodyPlan plan, DateTimeOffset occurred)
    {
        var status = context.Response.StatusCode;
        if (context.Features.Get<IForwarderErrorFeature>() is not null || !ShadowComparison.IsDecision(status))
        {
            // The legacy system never answered, or its answer decides nothing: there is nothing to compare with.
            return;
        }

        var request = context.Request;
        var skipped = plan == BodyPlan.Skip;
        var job = new ShadowJob(
            Guid.NewGuid(),
            rule,
            occurred,
            request.Method.ToUpperInvariant(),
            $"{request.PathBase.ToUriComponent()}{request.Path.ToUriComponent()}",
            request.QueryString.ToUriComponent(),
            CommandShapes.From(request.Method, request.Path.Value ?? string.Empty),
            TraceOf(request),
            skipped ? [] : ShadowHeaders.Select(request.Headers, rule.ForwardHeaders),
            plan == BodyPlan.Buffer ? await ReadBody(request) : null,
            status,
            skipped);

        queue.TryEnqueue(job);
    }
}
