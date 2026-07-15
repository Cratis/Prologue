// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Extractor.Capturing;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Cratis.Prologue.Extractor.Sources.Http;

/// <summary>
/// Represents a YARP transform provider that observes state-changing HTTP commands (<c>POST</c>, <c>PUT</c>,
/// <c>DELETE</c>) flowing through the reverse proxy and publishes them as observations. Only metadata is captured —
/// method, path, and the proxied response status — never the request or response body.
/// </summary>
/// <param name="channel">The channel observations are published to.</param>
public class CommandCaptureTransform(IObservationChannel channel) : ITransformProvider
{
    const string OccurredKey = "prologue.occurred";

    /// <inheritdoc/>
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    /// <inheritdoc/>
    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    /// <inheritdoc/>
    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(requestContext =>
        {
            requestContext.HttpContext.Items[OccurredKey] = DateTimeOffset.UtcNow;
            return ValueTask.CompletedTask;
        });

        context.AddResponseTransform(async responseContext =>
        {
            var http = responseContext.HttpContext;
            var method = http.Request.Method;
            if (!IsStateChanging(method))
            {
                return;
            }

            var occurred = http.Items[OccurredKey] is DateTimeOffset stamped ? stamped : DateTimeOffset.UtcNow;
            var path = $"{http.Request.Path}{http.Request.QueryString}";
            var traceId = TraceIdFrom(http.Request.Headers.TraceParent.ToString());
            var payload = new HttpCommandObserved(method, path, http.Response.StatusCode, traceId);
            await channel.Publish(new Observation(SourceKind.Http, occurred, payload), http.RequestAborted);
        });
    }

    static bool IsStateChanging(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method);

    // W3C traceparent is "version-<32 hex trace-id>-<16 hex span-id>-flags"; return the trace-id segment.
    static string TraceIdFrom(string traceParent)
    {
        var segments = traceParent.Split('-');
        return segments.Length >= 2 && segments[1].Length == 32 ? segments[1] : string.Empty;
    }
}
