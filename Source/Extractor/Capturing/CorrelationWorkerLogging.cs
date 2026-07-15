// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

internal static partial class CorrelationWorkerLog
{
    [LoggerMessage(LogLevel.Information, "Correlation worker started")]
    internal static partial void Started(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Stored capture {CaptureId} with {EntryCount} entries")]
    internal static partial void CaptureStored(ILogger logger, Guid captureId, int entryCount);
}
