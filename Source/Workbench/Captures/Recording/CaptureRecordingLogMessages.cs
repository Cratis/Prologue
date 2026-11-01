// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Captures.Recording;

/// <summary>
/// Log messages for <see cref="CaptureRecordingQueue"/> and <see cref="CaptureRecordingWorker"/>.
/// </summary>
internal static partial class CaptureRecordingLogMessages
{
    [LoggerMessage(LogLevel.Error, "The recording queue is full; capture {Capture} stays stored but is not recorded for the Workbench")]
    internal static partial void RecordingDropped(this ILogger<CaptureRecordingQueue> logger, Guid capture);

    [LoggerMessage(LogLevel.Error, "Recording capture {Capture} for the Workbench failed unexpectedly")]
    internal static partial void RecordingCrashed(this ILogger<CaptureRecordingWorker> logger, Guid capture, Exception exception);
}
