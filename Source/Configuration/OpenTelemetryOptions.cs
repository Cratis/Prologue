// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the configuration for the OpenTelemetry capture source. When enabled, the extractor acts as an OTLP
/// proxy (HTTP and gRPC): it captures span metadata and forwards the telemetry unchanged to the upstream collector
/// if one is configured.
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the OpenTelemetry OTLP proxy is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the upstream collector the received telemetry is forwarded to. Leave empty for a terminal
    /// capture (the extractor acts as the collector).
    /// </summary>
    public UpstreamOptions Upstream { get; set; } = new();

    /// <summary>
    /// Gets or sets the service names to capture. Empty captures spans from all services.
    /// </summary>
    public IList<string> ServiceNames { get; set; } = [];

    /// <summary>
    /// Gets or sets the span attribute keys whose values are captured. Attribute values outside this allowlist are
    /// never captured — only their keys.
    /// </summary>
    public IList<string> AttributeKeys { get; set; } = [];
}

/// <summary>
/// Represents the upstream OTLP collector endpoints telemetry is forwarded to.
/// </summary>
public class UpstreamOptions
{
    /// <summary>
    /// Gets or sets the base address of the upstream OTLP/HTTP collector (for example <c>http://collector:4318</c>).
    /// Empty disables HTTP forwarding.
    /// </summary>
    public string Http { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address of the upstream OTLP/gRPC collector (for example <c>http://collector:4317</c>).
    /// Empty disables gRPC forwarding.
    /// </summary>
    public string Grpc { get; set; } = string.Empty;
}
