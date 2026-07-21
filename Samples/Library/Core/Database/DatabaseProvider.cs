// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Core.Database;

/// <summary>
/// Represents the relational database engine the library system runs on. The same code runs on either — which one
/// is a deployment choice, and each shows the Prologue Extractor a different change-capture mechanism.
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// PostgreSQL, captured through logical replication. The default.
    /// </summary>
    Postgres,

    /// <summary>
    /// Microsoft SQL Server, captured through Change Data Capture.
    /// </summary>
    SqlServer
}
