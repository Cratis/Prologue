// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents one service the extractor captures from, resolved from configuration — the default service or a
/// named one — with the database sources that belong to it.
/// </summary>
/// <param name="Name">The name of the service; <see cref="ServiceName.Default"/> for the default service.</param>
/// <param name="SqlServer">The SQL Server change-capture sources belonging to the service.</param>
/// <param name="Postgres">The PostgreSQL change-capture sources belonging to the service.</param>
public record CaptureService(ServiceName Name, IReadOnlyList<SqlServerOptions> SqlServer, IReadOnlyList<PostgresOptions> Postgres);
