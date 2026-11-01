// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the root configuration for the Prologue Extractor capture tool, bound from the <c language="csharp">Prologue</c> configuration section.
/// </summary>
public class PrologueOptions
{
    /// <summary>
    /// The configuration section name the options are bound from.
    /// </summary>
    public const string SectionName = "Prologue";

    /// <summary>
    /// Gets or sets the Prologue the captures belong to. When set, captures are stamped with it and posted to the
    /// Receiver's Prologue-scoped endpoint, so they can later be interpreted on their own. Left unset
    /// (<see cref="Guid.Empty"/>), captures are not associated with any Prologue.
    /// </summary>
    public Guid PrologueId { get; set; }

    /// <summary>
    /// Gets or sets the output configuration — where captured data is written (Prologue Receiver or rolling JSON files).
    /// </summary>
    public OutputOptions Output { get; set; } = new();

    /// <summary>
    /// Gets or sets the correlation configuration.
    /// </summary>
    public CorrelationOptions Correlation { get; set; } = new();

    /// <summary>
    /// Gets or sets the SQL Server change-capture sources of the default service.
    /// </summary>
    public IList<SqlServerOptions> SqlServer { get; set; } = [];

    /// <summary>
    /// Gets or sets the PostgreSQL change-capture sources of the default service.
    /// </summary>
    public IList<PostgresOptions> Postgres { get; set; } = [];

    /// <summary>
    /// Gets or sets the OpenTelemetry OTLP proxy capture source. The proxy is shared by every service; its
    /// <see cref="OpenTelemetryOptions.ServiceNames"/> filter applies to the default service only.
    /// </summary>
    public OpenTelemetryOptions OpenTelemetry { get; set; } = new();

    /// <summary>
    /// Gets or sets the commands to shadow: a copy of each matching request is sent to a shadow destination after the legacy system
    /// has answered, and the two decisions are compared and reported to the Receiver as parity. Requires the Prologue the captures
    /// belong to and the Receiver as output.
    /// </summary>
    public IList<ShadowRuleOptions> Shadowing { get; set; } = [];

    /// <summary>
    /// Gets or sets the named services of a system made of several services, keyed by service name. Everything
    /// configured outside this section — the top-level databases, every reverse-proxy cluster no named service
    /// claims, and telemetry no named service claims — belongs to the default service, so a system captured as a
    /// single service needs no services at all.
    /// </summary>
    public IDictionary<string, ServiceOptions> Services { get; set; } = new Dictionary<string, ServiceOptions>(StringComparer.OrdinalIgnoreCase);
}
