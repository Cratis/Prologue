// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

internal static partial class CorrelationWorkerLog
{
    [LoggerMessage(LogLevel.Information, "Correlation worker started")]
    internal static partial void Started(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Stored capture {CaptureId} with {EntryCount} entries")]
    internal static partial void CaptureStored(ILogger logger, Guid captureId, int entryCount);

    [LoggerMessage(LogLevel.Error, "Capture {CaptureId} could not be stored and is lost")]
    internal static partial void StoreFailed(ILogger logger, Guid captureId, Exception exception);

    [LoggerMessage(LogLevel.Error, "An observation could not be correlated and is dropped")]
    internal static partial void ObservationFailed(ILogger logger, Exception exception);
}
