// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the root configuration for the Prologue Extractor capture tool, bound from the <c>Prologue</c> configuration section.
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
    /// Gets or sets the SQL Server change-capture sources.
    /// </summary>
    public IList<SqlServerOptions> SqlServer { get; set; } = [];

    /// <summary>
    /// Gets or sets the PostgreSQL change-capture sources.
    /// </summary>
    public IList<PostgresOptions> Postgres { get; set; } = [];

    /// <summary>
    /// Gets or sets the OpenTelemetry OTLP proxy capture source.
    /// </summary>
    public OpenTelemetryOptions OpenTelemetry { get; set; } = new();
}
