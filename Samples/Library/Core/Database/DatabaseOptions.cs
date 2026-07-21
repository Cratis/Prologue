// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Database;

/// <summary>
/// Represents how the library system talks to its database, bound from the <c>Library:Database</c> configuration
/// section. The Aspire composition sets these from whichever database it was asked to run.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// The configuration section name the options are bound from.
    /// </summary>
    public const string SectionName = "Library:Database";

    /// <summary>
    /// The name of the connection string the database is reached through.
    /// </summary>
    public const string ConnectionStringName = "library";

    /// <summary>
    /// Gets or sets the database engine to run on. PostgreSQL unless told otherwise.
    /// </summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Postgres;

    /// <summary>
    /// Gets or sets a value indicating whether the schema is created and prepared for change capture at startup.
    /// The sample owns an ephemeral database, so this is on by default.
    /// </summary>
    public bool InitializeOnStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the seed data is written once the schema exists.
    /// </summary>
    public bool SeedOnStartup { get; set; } = true;
}
