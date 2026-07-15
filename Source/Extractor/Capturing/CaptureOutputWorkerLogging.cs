// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

internal static partial class CaptureOutputWorkerLog
{
    [LoggerMessage(LogLevel.Information, "Capture output worker started")]
    internal static partial void Started(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Failed to write a batch of {Count} captures to the output")]
    internal static partial void WriteFailed(ILogger logger, int count, Exception exception);
}
