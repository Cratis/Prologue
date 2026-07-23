// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.Postgres;

internal static partial class PostgresSchemaCaptureLog
{
    [LoggerMessage(LogLevel.Information, "PostgreSQL source '{Source}' captured the schema of {TableCount} tables in '{Database}'")]
    internal static partial void SchemaCaptured(ILogger logger, string source, int tableCount, string database);

    [LoggerMessage(LogLevel.Warning, "PostgreSQL source '{Source}' could not capture the database schema; continuing without the structural evidence")]
    internal static partial void SchemaCaptureFailed(ILogger logger, string source, Exception exception);
}
