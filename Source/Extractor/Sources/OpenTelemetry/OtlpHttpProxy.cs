// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Represents the OTLP/HTTP receiver. It captures span metadata from <c>/v1/traces</c> (protobuf or JSON) and
/// forwards the raw payload to the upstream collector; <c>/v1/metrics</c> and <c>/v1/logs</c> are forwarded
/// transparently without capture so the proxy does not disrupt the monitored system's other signals.
/// </summary>
/// <param name="channel">The channel observations are published to.</param>
/// <param name="factory">The factory that maps spans to observations.</param>
/// <param name="httpClientFactory">The factory for the upstream forwarding client.</param>
/// <param name="options">The Prologue options carrying the upstream HTTP endpoint.</param>
/// <param name="logger">The logger.</param>
public class OtlpHttpProxy(
    IObservationChannel channel,
    SpanObservationFactory factory,
    IHttpClientFactory httpClientFactory,
    IOptions<PrologueOptions> options,
    ILogger<OtlpHttpProxy> logger)
{
    /// <summary>
    /// The named <see cref="HttpClient"/> used to forward to the upstream collector.
    /// </summary>
    public const string UpstreamClientName = "otlp-upstream";

    const string ProtobufContentType = "application/x-protobuf";

    /// <summary>
    /// Handles an OTLP/HTTP trace export — captures span metadata, then forwards the payload upstream.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleTraces(HttpContext context)
    {
        var body = await ReadBody(context);
        var isJson = (context.Request.ContentType ?? string.Empty).Contains("json", StringComparison.OrdinalIgnoreCase);

        try
        {
            var request = isJson
                ? JsonParser.Default.Parse<ExportTraceServiceRequest>(Encoding.UTF8.GetString(body))
                : ExportTraceServiceRequest.Parser.ParseFrom(body);

            foreach (var observation in factory.ToObservations(request))
            {
                await channel.Publish(observation, context.RequestAborted);
            }
        }
        catch (Exception exception)
        {
            OtlpHttpProxyLog.ParseFailed(logger, exception);
        }

        await Forward(context, "/v1/traces", body, isJson);
    }

    /// <summary>
    /// Forwards an OTLP/HTTP metrics export upstream without capture.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleMetrics(HttpContext context) => await ForwardOnly(context, "/v1/metrics");

    /// <summary>
    /// Forwards an OTLP/HTTP logs export upstream without capture.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleLogs(HttpContext context) => await ForwardOnly(context, "/v1/logs");

    static async Task<byte[]> ReadBody(HttpContext context)
    {
        await using var memory = new MemoryStream();
        await context.Request.Body.CopyToAsync(memory, context.RequestAborted);
        return memory.ToArray();
    }

    static async Task WriteEmptyResponse(HttpContext context, bool isJson)
    {
        var response = new ExportTraceServiceResponse();
        context.Response.StatusCode = StatusCodes.Status200OK;
        if (isJson)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonFormatter.Default.Format(response), context.RequestAborted);
        }
        else
        {
            context.Response.ContentType = ProtobufContentType;
            await context.Response.Body.WriteAsync(response.ToByteArray(), context.RequestAborted);
        }
    }

    async Task ForwardOnly(HttpContext context, string path)
    {
        var body = await ReadBody(context);
        var isJson = (context.Request.ContentType ?? string.Empty).Contains("json", StringComparison.OrdinalIgnoreCase);
        await Forward(context, path, body, isJson);
    }

    async Task Forward(HttpContext context, string path, byte[] body, bool isJson)
    {
        var upstream = options.Value.OpenTelemetry.Upstream.Http;
        var contentType = context.Request.ContentType ?? (isJson ? "application/json" : ProtobufContentType);

        if (string.IsNullOrWhiteSpace(upstream))
        {
            // Terminal capture — acknowledge with an empty export response in the requested encoding.
            await WriteEmptyResponse(context, isJson);
            return;
        }

        try
        {
            var client = httpClientFactory.CreateClient(UpstreamClientName);
            using var content = new ByteArrayContent(body);
            content.Headers.TryAddWithoutValidation("Content-Type", contentType);
            using var response = await client.PostAsync($"{upstream.TrimEnd('/')}{path}", content, context.RequestAborted);
            context.Response.StatusCode = (int)response.StatusCode;
            var responseBody = await response.Content.ReadAsByteArrayAsync(context.RequestAborted);
            if (responseBody.Length > 0)
            {
                context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? contentType;
                await context.Response.Body.WriteAsync(responseBody, context.RequestAborted);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            OtlpHttpProxyLog.ForwardFailed(logger, path, exception);
            await WriteEmptyResponse(context, isJson);
        }
    }
}
