// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

internal static partial class BufferedCaptureStoreLog
{
    [LoggerMessage(LogLevel.Warning, "Capture buffer is full; dropped capture {CaptureId}")]
    internal static partial void CaptureDropped(ILogger logger, Guid captureId);
}
