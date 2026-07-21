// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Maps OTLP trace export requests into <see cref="Observation"/>s, capturing span metadata only — plus the values
/// of an allowlisted set of attributes. Applies the configured service-name filter.
/// </summary>
/// <param name="options">The OpenTelemetry options carrying the service-name filter and attribute allowlist.</param>
public class SpanObservationFactory(OpenTelemetryOptions options)
{
    readonly OtlpCaptureFilter _filter = new(options);

    /// <summary>
    /// Produces observations for every captured span in an OTLP trace export request.
    /// </summary>
    /// <param name="request">The OTLP export request.</param>
    /// <returns>The observations for the spans that pass the service-name filter.</returns>
    public IEnumerable<Observation> ToObservations(ExportTraceServiceRequest request)
    {
        foreach (var resourceSpans in request.ResourceSpans)
        {
            var serviceName = OtlpCaptureFilter.ServiceNameOf(resourceSpans.Resource?.Attributes);

            if (!_filter.CapturesService(serviceName))
            {
                continue;
            }

            foreach (var span in resourceSpans.ScopeSpans.SelectMany(scopeSpans => scopeSpans.Spans))
            {
                yield return ToObservation(span, serviceName);
            }
        }
    }

    static string Kind(Span.Types.SpanKind kind) => kind.ToString().Replace("Unspecified", "Internal", StringComparison.Ordinal);

    Observation ToObservation(Span span, string serviceName)
    {
        var occurred = DateTimeOffset.UnixEpoch.AddTicks((long)(span.StartTimeUnixNano / 100));
        var durationMilliseconds = span.EndTimeUnixNano > span.StartTimeUnixNano
            ? (long)((span.EndTimeUnixNano - span.StartTimeUnixNano) / 1_000_000)
            : 0;

        var payload = new TelemetryObserved(
            Convert.ToHexStringLower(span.TraceId.Span),
            Convert.ToHexStringLower(span.SpanId.Span),
            span.ParentSpanId.IsEmpty ? string.Empty : Convert.ToHexStringLower(span.ParentSpanId.Span),
            span.Name,
            Kind(span.Kind),
            serviceName,
            span.Status is null ? 0 : (int)span.Status.Code,
            durationMilliseconds,
            OtlpCaptureFilter.KeysOf(span.Attributes),
            _filter.AllowlistedValuesOf(span.Attributes));

        return new Observation(SourceKind.OpenTelemetry, occurred, payload);
    }
}
