// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the metadata of a single OpenTelemetry log record observed flowing through the engine — that
/// something was logged, at what severity, and as part of which trace. The log body is deliberately never
/// captured: it is free text that routinely carries payload data and personal information. Only metadata is
/// captured: severity, the producing service and scope, the trace it belongs to, attribute names, and the values
/// of an allowlisted set of attributes.
/// </summary>
/// <param name="TraceId">The W3C trace id the log record belongs to, used to correlate it with the command that caused it; empty when absent.</param>
/// <param name="SpanId">The span the log record was written within, or empty when absent.</param>
/// <param name="SeverityText">The severity as text (for example <c>Information</c>, <c>Warning</c>, <c>Error</c>).</param>
/// <param name="SeverityNumber">The numeric OpenTelemetry severity (1 trace through 24 fatal).</param>
/// <param name="ServiceName">The name of the service that produced the log record.</param>
/// <param name="ScopeName">The name of the instrumentation scope (typically the logger category).</param>
/// <param name="AttributeKeys">The names of the log record's attributes.</param>
/// <param name="Attributes">The values of the allowlisted attributes only.</param>
public record LogObserved(
    string TraceId,
    string SpanId,
    string SeverityText,
    int SeverityNumber,
    string ServiceName,
    string ScopeName,
    IReadOnlyList<string> AttributeKeys,
    IReadOnlyDictionary<string, string> Attributes) : ObservationPayload;
