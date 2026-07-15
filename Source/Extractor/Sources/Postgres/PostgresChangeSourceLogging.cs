// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.Postgres;

internal static partial class PostgresChangeSourceLog
{
    [LoggerMessage(LogLevel.Information, "PostgreSQL source '{Source}' watching via replication slot '{Slot}'")]
    internal static partial void Watching(ILogger logger, string source, string slot);

    [LoggerMessage(LogLevel.Error, "PostgreSQL source '{Source}' failed; retrying")]
    internal static partial void WatchFailed(ILogger logger, string source, Exception exception);
}
