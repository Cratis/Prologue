// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Maps OTLP trace export requests into <see cref="Observation"/>s, capturing span metadata only — plus the values
/// of an allowlisted set of attributes. Applies the configured service-name filter.
/// </summary>
/// <param name="options">The OpenTelemetry options carrying the service-name filter and attribute allowlist.</param>
public class SpanObservationFactory(OpenTelemetryOptions options)
{
    const string ServiceNameAttribute = "service.name";

    readonly HashSet<string> _serviceNames = new(options.ServiceNames, StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> _attributeKeys = new(options.AttributeKeys, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Produces observations for every captured span in an OTLP trace export request.
    /// </summary>
    /// <param name="request">The OTLP export request.</param>
    /// <returns>The observations for the spans that pass the service-name filter.</returns>
    public IEnumerable<Observation> ToObservations(ExportTraceServiceRequest request)
    {
        foreach (var resourceSpans in request.ResourceSpans)
        {
            var serviceName = ServiceNameOf(resourceSpans.Resource?.Attributes);
            if (_serviceNames.Count > 0 && !_serviceNames.Contains(serviceName))
            {
                continue;
            }

            foreach (var scopeSpans in resourceSpans.ScopeSpans)
            {
                foreach (var span in scopeSpans.Spans)
                {
                    yield return ToObservation(span, serviceName);
                }
            }
        }
    }

    static string ServiceNameOf(IEnumerable<KeyValue>? attributes)
    {
        var value = attributes?.FirstOrDefault(attribute => attribute.Key == ServiceNameAttribute)?.Value;
        return value is null ? string.Empty : StringValueOf(value);
    }

    static string StringValueOf(AnyValue value) => value.ValueCase switch
    {
        AnyValue.ValueOneofCase.StringValue => value.StringValue,
        AnyValue.ValueOneofCase.BoolValue => value.BoolValue ? "true" : "false",
        AnyValue.ValueOneofCase.IntValue => value.IntValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
        AnyValue.ValueOneofCase.DoubleValue => value.DoubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
        _ => string.Empty,
    };

    static string Kind(Span.Types.SpanKind kind) => kind.ToString().Replace("Unspecified", "Internal", StringComparison.Ordinal);

    Observation ToObservation(Span span, string serviceName)
    {
        var occurred = DateTimeOffset.UnixEpoch.AddTicks((long)(span.StartTimeUnixNano / 100));
        var durationMilliseconds = span.EndTimeUnixNano > span.StartTimeUnixNano
            ? (long)((span.EndTimeUnixNano - span.StartTimeUnixNano) / 1_000_000)
            : 0;

        var attributes = span.Attributes
            .Where(attribute => _attributeKeys.Contains(attribute.Key))
            .GroupBy(attribute => attribute.Key)
            .ToDictionary(group => group.Key, group => StringValueOf(group.Last().Value));

        var payload = new TelemetryObserved(
            Convert.ToHexStringLower(span.TraceId.Span),
            Convert.ToHexStringLower(span.SpanId.Span),
            span.ParentSpanId.IsEmpty ? string.Empty : Convert.ToHexStringLower(span.ParentSpanId.Span),
            span.Name,
            Kind(span.Kind),
            serviceName,
            span.Status is null ? 0 : (int)span.Status.Code,
            durationMilliseconds,
            [.. span.Attributes.Select(attribute => attribute.Key)],
            attributes);

        return new Observation(SourceKind.OpenTelemetry, occurred, payload);
    }
}
