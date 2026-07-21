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
    /// Gets or sets a value indicating whether the extractor enables Change Data Capture itself — on the database
    /// and on the tables it should watch — when it is not already enabled. Prologue captures systems that were
    /// built without knowing it exists, so preparing the database is the extractor's job, not the captured
    /// system's. Turn it off to leave the database exactly as it is and rely on CDC a DBA enabled beforehand.
    /// </summary>
    public bool EnableChangeDataCapture { get; set; } = true;

    /// <summary>
    /// Gets or sets the tables to enable Change Data Capture on, as <c>schema.table</c> or just <c>table</c> for
    /// the default schema. Empty enables it on every user table in the database.
    /// </summary>
    public IList<string> Tables { get; set; } = [];

    /// <summary>
    /// Gets or sets the interval, in milliseconds, between polls of the CDC change tables.
    /// </summary>
    public int PollIntervalMilliseconds { get; set; } = 500;

    /// <summary>
    /// Gets the poll interval as a <see cref="TimeSpan"/>.
    /// </summary>
    public TimeSpan PollInterval => TimeSpan.FromMilliseconds(PollIntervalMilliseconds);
}
