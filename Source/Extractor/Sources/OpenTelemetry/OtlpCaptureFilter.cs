// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using OpenTelemetry.Proto.Common.V1;

namespace Cratis.Prologue.Extractor.Sources.OpenTelemetry;

/// <summary>
/// Applies the configured capture policy to OTLP telemetry — which services are captured, which captured service
/// the telemetry belongs to, and which attribute values may be recorded. Shared by the span, metric, and log observation factories so all three signals honor
/// the same privacy rules: attribute names are always captured, attribute values only when allowlisted.
/// </summary>
/// <param name="options">The OpenTelemetry options carrying the attribute allowlist.</param>
/// <param name="services">The services captured from, resolving which service telemetry belongs to.</param>
public sealed class OtlpCaptureFilter(OpenTelemetryOptions options, CaptureServices services)
{
    const string ServiceNameAttribute = "service.name";

    readonly HashSet<string> _attributeKeys = new(options.AttributeKeys, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves the <c language="csharp">service.name</c> resource attribute.
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
    /// Determines whether telemetry from the given service is captured, and which captured service it belongs to.
    /// </summary>
    /// <param name="serviceName">The <c language="csharp">service.name</c> the telemetry was reported under.</param>
    /// <param name="service">The captured service the telemetry belongs to.</param>
    /// <returns>True when the telemetry is captured.</returns>
    public bool CapturesService(string serviceName, out ServiceName service) =>
        services.TryResolveTelemetry(serviceName, out service);

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
