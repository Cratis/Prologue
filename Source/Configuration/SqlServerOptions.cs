// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the configuration for a single SQL Server change-capture source.
/// </summary>
public class SqlServerOptions
{
    /// <summary>
    /// Gets or sets the logical name identifying this source in captures.
    /// </summary>
    public string Name { get; set; } = "sqlserver";

    /// <summary>
    /// Gets or sets the connection string to the SQL Server database that has CDC enabled.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the interval, in milliseconds, between polls of the CDC change tables.
    /// </summary>
    public int PollIntervalMilliseconds { get; set; } = 500;

    /// <summary>
    /// Gets the poll interval as a <see cref="TimeSpan"/>.
    /// </summary>
    public TimeSpan PollInterval => TimeSpan.FromMilliseconds(PollIntervalMilliseconds);
}
