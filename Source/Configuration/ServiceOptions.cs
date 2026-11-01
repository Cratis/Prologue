// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the configuration for one named service of a system made of several services. A named service has
/// its own databases, receives the HTTP traffic of its own reverse-proxy clusters and owns the telemetry of its own
/// OpenTelemetry service names; observations from different services are never correlated into one capture.
/// </summary>
public class ServiceOptions
{
    /// <summary>
    /// Gets or sets the SQL Server change-capture sources belonging to the service.
    /// </summary>
    public IList<SqlServerOptions> SqlServer { get; set; } = [];

    /// <summary>
    /// Gets or sets the PostgreSQL change-capture sources belonging to the service.
    /// </summary>
    public IList<PostgresOptions> Postgres { get; set; } = [];

    /// <summary>
    /// Gets or sets the ids of the reverse-proxy clusters whose HTTP commands belong to the service. Empty means
    /// the cluster whose id is the service's own name.
    /// </summary>
    public IList<string> Clusters { get; set; } = [];

    /// <summary>
    /// Gets or sets the OpenTelemetry <c language="csharp">service.name</c> values whose telemetry belongs to the
    /// service. Empty means the service's own name.
    /// </summary>
    public IList<string> TelemetryServiceNames { get; set; } = [];
}
