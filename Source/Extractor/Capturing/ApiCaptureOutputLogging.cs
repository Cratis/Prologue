// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

internal static partial class ApiCaptureOutputLog
{
    [LoggerMessage(LogLevel.Error, "Failed to send capture {CaptureId} to the Prologue Receiver")]
    internal static partial void SendFailed(ILogger logger, Guid captureId, Exception exception);
}
