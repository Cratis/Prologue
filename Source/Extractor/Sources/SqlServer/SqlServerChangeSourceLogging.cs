// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

internal static partial class SqlServerChangeSourceLog
{
    [LoggerMessage(LogLevel.Information, "SQL Server source '{Source}' watching {InstanceCount} capture instances in '{Database}'")]
    internal static partial void Watching(ILogger logger, string source, int instanceCount, string database);

    [LoggerMessage(LogLevel.Error, "SQL Server source '{Source}' failed; retrying")]
    internal static partial void WatchFailed(ILogger logger, string source, Exception exception);
}
