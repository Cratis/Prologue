// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the metadata of a single OpenTelemetry span observed flowing through the engine — the intent (an
/// operation, often a command) and its place in a trace. Only metadata is captured: the span name, kind, timing,
/// trace/span identifiers, attribute names, and the values of an allowlisted set of attributes — never arbitrary
/// attribute values.
/// </summary>
/// <param name="TraceId">The W3C trace id the span belongs to, used to correlate intent with the events it produces.</param>
/// <param name="SpanId">The span's identifier.</param>
/// <param name="ParentSpanId">The parent span's identifier, or empty when the span is a root.</param>
/// <param name="Name">The span name (the operation or command).</param>
/// <param name="Kind">The span kind (for example <c>Server</c>, <c>Client</c>, <c>Producer</c>, <c>Consumer</c>, <c>Internal</c>).</param>
/// <param name="ServiceName">The name of the service that produced the span.</param>
/// <param name="StatusCode">The span status code (0 unset, 1 ok, 2 error).</param>
/// <param name="DurationMilliseconds">The span duration in milliseconds.</param>
/// <param name="AttributeKeys">The names of the span's attributes.</param>
/// <param name="Attributes">The values of the allowlisted attributes only.</param>
public record TelemetryObserved(
    string TraceId,
    string SpanId,
    string ParentSpanId,
    string Name,
    string Kind,
    string ServiceName,
    int StatusCode,
    long DurationMilliseconds,
    IReadOnlyList<string> AttributeKeys,
    IReadOnlyDictionary<string, string> Attributes) : ObservationPayload;
