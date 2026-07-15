// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Configuration;

/// <summary>
/// Represents the configuration for a single PostgreSQL logical-replication change-capture source.
/// </summary>
public class PostgresOptions
{
    /// <summary>
    /// Gets or sets the logical name identifying this source in captures.
    /// </summary>
    public string Name { get; set; } = "postgres";

    /// <summary>
    /// Gets or sets the connection string to the PostgreSQL database. The connecting role must have the
    /// <c>REPLICATION</c> attribute and the database must have <c>wal_level = logical</c>.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the logical replication slot to create and consume.
    /// </summary>
    public string Slot { get; set; } = "prologue_slot";

    /// <summary>
    /// Gets or sets the name of the publication the slot streams changes for.
    /// </summary>
    public string Publication { get; set; } = "prologue_publication";
}
