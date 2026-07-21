// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Configuration;
using OpenTelemetry.Proto.Common.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Applies the configured capture policy to OTLP telemetry — which services are captured and which attribute
/// values may be recorded. Shared by the span, metric, and log observation factories so all three signals honor
/// the same privacy rules: attribute names are always captured, attribute values only when allowlisted.
/// </summary>
/// <param name="options">The OpenTelemetry options carrying the service-name filter and attribute allowlist.</param>
public sealed class OtlpCaptureFilter(OpenTelemetryOptions options)
{
    const string ServiceNameAttribute = "service.name";

    readonly HashSet<string> _serviceNames = new(options.ServiceNames, StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> _attributeKeys = new(options.AttributeKeys, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves the <c>service.name</c> resource attribute.
    /// </summary>
    /// <param name="attributes">The resource attributes to resolve from.</param>
    /// <returns>The service name, or an empty string when absent.</returns>
    public static string ServiceNameOf(IEnumerable<KeyValue>? attributes)
    {
        var value = attributes?.FirstOrDefault(attribute => attribute.Key == ServiceNameAttribute)?.Value;
        return value is null ? string.Empty : StringValueOf(value);
    }

    /// <summary>
    /// Renders an OTLP value as a string, for the scalar cases a captured attribute value can take.
    /// </summary>
    /// <param name="value">The value to render.</param>
    /// <returns>The rendered value, or an empty string for non-scalar values.</returns>
    public static string StringValueOf(AnyValue value) => value.ValueCase switch
    {
        AnyValue.ValueOneofCase.StringValue => value.StringValue,
        AnyValue.ValueOneofCase.BoolValue => value.BoolValue ? "true" : "false",
        AnyValue.ValueOneofCase.IntValue => value.IntValue.ToString(CultureInfo.InvariantCulture),
        AnyValue.ValueOneofCase.DoubleValue => value.DoubleValue.ToString(CultureInfo.InvariantCulture),
        _ => string.Empty,
    };

    /// <summary>
    /// Lists the names of the given attributes — always captured, since names describe the shape of the system
    /// rather than its data.
    /// </summary>
    /// <param name="attributes">The attributes to list the names of.</param>
    /// <returns>The distinct attribute names, in the order they first appear.</returns>
    public static IReadOnlyList<string> KeysOf(IEnumerable<KeyValue> attributes) =>
        [.. attributes.Select(attribute => attribute.Key).Distinct(StringComparer.Ordinal)];

    /// <summary>
    /// Determines whether telemetry from the given service is captured.
    /// </summary>
    /// <param name="serviceName">The name of the service that produced the telemetry.</param>
    /// <returns>True when the service passes the filter; an empty filter captures every service.</returns>
    public bool CapturesService(string serviceName) =>
        _serviceNames.Count == 0 || _serviceNames.Contains(serviceName);

    /// <summary>
    /// Selects the values of the allowlisted attributes only. Values outside the allowlist are never captured.
    /// </summary>
    /// <param name="attributes">The attributes to select from.</param>
    /// <returns>The allowlisted attribute values, keyed by attribute name.</returns>
    public IReadOnlyDictionary<string, string> AllowlistedValuesOf(IEnumerable<KeyValue> attributes) =>
        attributes
            .Where(attribute => _attributeKeys.Contains(attribute.Key))
            .GroupBy(attribute => attribute.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => StringValueOf(group.Last().Value), StringComparer.Ordinal);
}
