// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using OpenTelemetry.Proto.Collector.Logs.V1;
using OpenTelemetry.Proto.Logs.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Maps OTLP log export requests into <see cref="Observation"/>s, capturing log metadata only. The log body is
/// never captured — it is free text that routinely carries payload data and personal information. Applies the
/// configured service-name filter.
/// </summary>
/// <param name="options">The OpenTelemetry options carrying the service-name filter and attribute allowlist.</param>
public class LogObservationFactory(OpenTelemetryOptions options)
{
    readonly OtlpCaptureFilter _filter = new(options);

    /// <summary>
    /// Produces observations for every captured log record in an OTLP log export request.
    /// </summary>
    /// <param name="request">The OTLP export request.</param>
    /// <returns>The observations for the log records that pass the service-name filter.</returns>
    public IEnumerable<Observation> ToObservations(ExportLogsServiceRequest request)
    {
        foreach (var resourceLogs in request.ResourceLogs)
        {
            var serviceName = OtlpCaptureFilter.ServiceNameOf(resourceLogs.Resource?.Attributes);

            if (!_filter.CapturesService(serviceName))
            {
                continue;
            }

            foreach (var scopeLogs in resourceLogs.ScopeLogs)
            {
                var scopeName = scopeLogs.Scope?.Name ?? string.Empty;

                foreach (var record in scopeLogs.LogRecords)
                {
                    yield return ToObservation(record, serviceName, scopeName);
                }
            }
        }
    }

    Observation ToObservation(LogRecord record, string serviceName, string scopeName)
    {
        var timestamp = record.TimeUnixNano > 0 ? record.TimeUnixNano : record.ObservedTimeUnixNano;
        var occurred = timestamp > 0
            ? DateTimeOffset.UnixEpoch.AddTicks((long)(timestamp / 100))
            : DateTimeOffset.UtcNow;

        var payload = new LogObserved(
            record.TraceId.IsEmpty ? string.Empty : Convert.ToHexStringLower(record.TraceId.Span),
            record.SpanId.IsEmpty ? string.Empty : Convert.ToHexStringLower(record.SpanId.Span),
            record.SeverityText,
            (int)record.SeverityNumber,
            serviceName,
            scopeName,
            OtlpCaptureFilter.KeysOf(record.Attributes),
            _filter.AllowlistedValuesOf(record.Attributes));

        return new Observation(SourceKind.OpenTelemetry, occurred, payload);
    }
}
