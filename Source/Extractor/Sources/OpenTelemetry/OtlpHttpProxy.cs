// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using OpenTelemetry.Proto.Collector.Logs.V1;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Represents the OTLP/HTTP receiver. It captures metadata from all three signals — traces, metrics, and logs —
/// and forwards the raw payload unchanged to the upstream collector, so the proxy never disrupts the monitored
/// system's telemetry.
/// </summary>
/// <param name="channel">The channel observations are published to.</param>
/// <param name="spans">The factory that maps spans to observations.</param>
/// <param name="metrics">The factory that maps metrics to observations.</param>
/// <param name="logs">The factory that maps log records to observations.</param>
/// <param name="httpClientFactory">The factory for the upstream forwarding client.</param>
/// <param name="options">The Prologue options carrying the upstream HTTP endpoint.</param>
/// <param name="logger">The logger.</param>
public class OtlpHttpProxy(
    IObservationChannel channel,
    SpanObservationFactory spans,
    MetricObservationFactory metrics,
    LogObservationFactory logs,
    IHttpClientFactory httpClientFactory,
    IOptions<PrologueOptions> options,
    ILogger<OtlpHttpProxy> logger)
{
    /// <summary>
    /// The named <see cref="HttpClient"/> used to forward to the upstream collector.
    /// </summary>
    public const string UpstreamClientName = "otlp-upstream";

    /// <summary>
    /// The OTLP/HTTP path traces are exported to.
    /// </summary>
    public const string TracesPath = "/v1/traces";

    /// <summary>
    /// The OTLP/HTTP path metrics are exported to.
    /// </summary>
    public const string MetricsPath = "/v1/metrics";

    /// <summary>
    /// The OTLP/HTTP path logs are exported to.
    /// </summary>
    public const string LogsPath = "/v1/logs";

    const string ProtobufContentType = "application/x-protobuf";

    /// <summary>
    /// Handles an OTLP/HTTP trace export — captures span metadata, then forwards the payload upstream.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task HandleTraces(HttpContext context) =>
        Handle(context, TracesPath, ExportTraceServiceRequest.Parser, spans.ToObservations, new ExportTraceServiceResponse());

    /// <summary>
    /// Handles an OTLP/HTTP metric export — captures metric metadata, then forwards the payload upstream.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task HandleMetrics(HttpContext context) =>
        Handle(context, MetricsPath, ExportMetricsServiceRequest.Parser, metrics.ToObservations, new ExportMetricsServiceResponse());

    /// <summary>
    /// Handles an OTLP/HTTP log export — captures log metadata, then forwards the payload upstream.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task HandleLogs(HttpContext context) =>
        Handle(context, LogsPath, ExportLogsServiceRequest.Parser, logs.ToObservations, new ExportLogsServiceResponse());

    static async Task<byte[]> ReadBody(HttpContext context)
    {
        await using var memory = new MemoryStream();
        await context.Request.Body.CopyToAsync(memory, context.RequestAborted);
        return memory.ToArray();
    }

    static bool IsJsonRequest(HttpContext context) =>
        (context.Request.ContentType ?? string.Empty).Contains("json", StringComparison.OrdinalIgnoreCase);

    static async Task WriteEmptyResponse(HttpContext context, bool isJson, IMessage response)
    {
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

    async Task Handle<TRequest>(
        HttpContext context,
        string path,
        MessageParser<TRequest> parser,
        Func<TRequest, IEnumerable<Observation>> toObservations,
        IMessage emptyResponse)
        where TRequest : IMessage<TRequest>, new()
    {
        var body = await ReadBody(context);
        var isJson = IsJsonRequest(context);

        try
        {
            var request = isJson
                ? JsonParser.Default.Parse<TRequest>(Encoding.UTF8.GetString(body))
                : parser.ParseFrom(body);

            foreach (var observation in toObservations(request))
            {
                await channel.Publish(observation, context.RequestAborted);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            OtlpHttpProxyLog.ParseFailed(logger, path, exception);
        }

        await Forward(context, path, body, isJson, emptyResponse);
    }

    async Task Forward(HttpContext context, string path, byte[] body, bool isJson, IMessage emptyResponse)
    {
        var upstream = options.Value.OpenTelemetry.Upstream.Http;
        var contentType = context.Request.ContentType ?? (isJson ? "application/json" : ProtobufContentType);

        if (string.IsNullOrWhiteSpace(upstream))
        {
            // Terminal capture — acknowledge with an empty export response in the requested encoding.
            await WriteEmptyResponse(context, isJson, emptyResponse);
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
            await WriteEmptyResponse(context, isJson, emptyResponse);
        }
    }
}
