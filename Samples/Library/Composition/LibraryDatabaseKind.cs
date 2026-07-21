// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Library.Composition;

/// <summary>
/// Represents the database engine the composition was asked to run the library on, chosen with
/// <c>--database postgres|mssql</c>.
/// </summary>
/// <param name="Provider">The value the Core service binds its <c>Library:Database:Provider</c> option from.</param>
/// <param name="IsSqlServer">Whether SQL Server was chosen.</param>
public readonly record struct LibraryDatabaseKind(string Provider, bool IsSqlServer)
{
    /// <summary>
    /// Resolves the database kind from the composition's command-line arguments.
    /// </summary>
    /// <param name="configuration">The configuration the arguments were bound into.</param>
    /// <returns>The chosen database kind; PostgreSQL when nothing was asked for.</returns>
    public static LibraryDatabaseKind From(IConfiguration configuration)
    {
        var requested = configuration["database"] ?? "postgres";

        return requested.ToLowerInvariant() switch
        {
            "mssql" or "sqlserver" or "sql" => new LibraryDatabaseKind("SqlServer", true),
            _ => new LibraryDatabaseKind("Postgres", false),
        };
    }
}
