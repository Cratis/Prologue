// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Metrics.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Maps OTLP metric export requests into <see cref="Observation"/>s, capturing metric metadata only — what the
/// system measures about itself, never the measured values. Applies the configured service-name filter.
/// </summary>
/// <param name="options">The OpenTelemetry options carrying the service-name filter and attribute allowlist.</param>
public class MetricObservationFactory(OpenTelemetryOptions options)
{
    readonly OtlpCaptureFilter _filter = new(options);

    /// <summary>
    /// Produces observations for every captured metric in an OTLP metric export request.
    /// </summary>
    /// <param name="request">The OTLP export request.</param>
    /// <returns>The observations for the metrics that pass the service-name filter.</returns>
    public IEnumerable<Observation> ToObservations(ExportMetricsServiceRequest request)
    {
        foreach (var resourceMetrics in request.ResourceMetrics)
        {
            var serviceName = OtlpCaptureFilter.ServiceNameOf(resourceMetrics.Resource?.Attributes);

            if (!_filter.CapturesService(serviceName))
            {
                continue;
            }

            foreach (var metric in resourceMetrics.ScopeMetrics.SelectMany(scopeMetrics => scopeMetrics.Metrics))
            {
                yield return ToObservation(metric, serviceName);
            }
        }
    }

    static string Kind(Metric metric) => metric.DataCase switch
    {
        Metric.DataOneofCase.Gauge => "Gauge",
        Metric.DataOneofCase.Sum => "Sum",
        Metric.DataOneofCase.Histogram => "Histogram",
        Metric.DataOneofCase.ExponentialHistogram => "ExponentialHistogram",
        Metric.DataOneofCase.Summary => "Summary",
        _ => "Unknown",
    };

    // The data points are what carries the measurements. Only their count and attribute shape are captured — the
    // values themselves describe how the system performs, not how it is structured, and are deliberately dropped.
    static IReadOnlyList<KeyValue> AttributesOf(Metric metric) => metric.DataCase switch
    {
        Metric.DataOneofCase.Gauge => [.. metric.Gauge.DataPoints.SelectMany(point => point.Attributes)],
        Metric.DataOneofCase.Sum => [.. metric.Sum.DataPoints.SelectMany(point => point.Attributes)],
        Metric.DataOneofCase.Histogram => [.. metric.Histogram.DataPoints.SelectMany(point => point.Attributes)],
        Metric.DataOneofCase.ExponentialHistogram => [.. metric.ExponentialHistogram.DataPoints.SelectMany(point => point.Attributes)],
        Metric.DataOneofCase.Summary => [.. metric.Summary.DataPoints.SelectMany(point => point.Attributes)],
        _ => [],
    };

    static int DataPointCountOf(Metric metric) => metric.DataCase switch
    {
        Metric.DataOneofCase.Gauge => metric.Gauge.DataPoints.Count,
        Metric.DataOneofCase.Sum => metric.Sum.DataPoints.Count,
        Metric.DataOneofCase.Histogram => metric.Histogram.DataPoints.Count,
        Metric.DataOneofCase.ExponentialHistogram => metric.ExponentialHistogram.DataPoints.Count,
        Metric.DataOneofCase.Summary => metric.Summary.DataPoints.Count,
        _ => 0,
    };

    static ulong StartTimeOf(Metric metric) => metric.DataCase switch
    {
        Metric.DataOneofCase.Gauge => metric.Gauge.DataPoints.FirstOrDefault()?.TimeUnixNano ?? 0,
        Metric.DataOneofCase.Sum => metric.Sum.DataPoints.FirstOrDefault()?.TimeUnixNano ?? 0,
        Metric.DataOneofCase.Histogram => metric.Histogram.DataPoints.FirstOrDefault()?.TimeUnixNano ?? 0,
        Metric.DataOneofCase.ExponentialHistogram => metric.ExponentialHistogram.DataPoints.FirstOrDefault()?.TimeUnixNano ?? 0,
        Metric.DataOneofCase.Summary => metric.Summary.DataPoints.FirstOrDefault()?.TimeUnixNano ?? 0,
        _ => 0,
    };

    Observation ToObservation(Metric metric, string serviceName)
    {
        var timestamp = StartTimeOf(metric);
        var occurred = timestamp > 0
            ? DateTimeOffset.UnixEpoch.AddTicks((long)(timestamp / 100))
            : DateTimeOffset.UtcNow;

        var attributes = AttributesOf(metric);

        var payload = new MetricObserved(
            metric.Name,
            Kind(metric),
            metric.Unit,
            serviceName,
            DataPointCountOf(metric),
            OtlpCaptureFilter.KeysOf(attributes),
            _filter.AllowlistedValuesOf(attributes));

        return new Observation(SourceKind.OpenTelemetry, occurred, payload);
    }
}
