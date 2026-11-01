// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The database engines changes can be captured from.
/// </summary>
public enum DatabaseEngine
{
    /// <summary>
    /// SQL Server, read through change data capture.
    /// </summary>
    SqlServer,

    /// <summary>
    /// PostgreSQL, read through logical replication.
    /// </summary>
    PostgreSql
}
