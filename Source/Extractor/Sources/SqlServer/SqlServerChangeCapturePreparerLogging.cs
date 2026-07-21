// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

internal static partial class SqlServerChangeCapturePreparerLog
{
    [LoggerMessage(LogLevel.Information, "SQL Server source '{Source}' ensured Change Data Capture on {TableCount} tables")]
    internal static partial void Enabled(ILogger logger, string source, int tableCount);

    [LoggerMessage(LogLevel.Warning, "SQL Server source '{Source}' could not enable Change Data Capture — it needs sysadmin and a running SQL Server Agent. Continuing with whatever capture instances already exist; enable CDC manually with Source/Extractor/sql/enable-sqlserver-cdc.sql if nothing is captured")]
    internal static partial void PreparationFailed(ILogger logger, string source, Exception exception);
}
