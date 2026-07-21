// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the metadata of a single OpenTelemetry metric observed flowing through the engine — what the system
/// measures about itself. Only metadata is captured: the metric name, kind, unit, the producing service, how many
/// data points arrived, attribute names, and the values of an allowlisted set of attributes — never the measured
/// values themselves.
/// </summary>
/// <param name="Name">The metric name (for example <c>http.server.request.duration</c>).</param>
/// <param name="Kind">The metric kind (<c>Gauge</c>, <c>Sum</c>, <c>Histogram</c>, <c>ExponentialHistogram</c>, or <c>Summary</c>).</param>
/// <param name="Unit">The unit of the measurement, or empty when unspecified.</param>
/// <param name="ServiceName">The name of the service that produced the metric.</param>
/// <param name="DataPointCount">The number of data points carried in the export for this metric.</param>
/// <param name="AttributeKeys">The names of the attributes across the metric's data points.</param>
/// <param name="Attributes">The values of the allowlisted attributes only.</param>
public record MetricObserved(
    string Name,
    string Kind,
    string Unit,
    string ServiceName,
    int DataPointCount,
    IReadOnlyList<string> AttributeKeys,
    IReadOnlyDictionary<string, string> Attributes) : ObservationPayload;
