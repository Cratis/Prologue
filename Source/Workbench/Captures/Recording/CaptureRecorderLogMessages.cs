// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Captures.Recording;

/// <summary>
/// Log messages for <see cref="CaptureRecorder"/>.
/// </summary>
internal static partial class CaptureRecorderLogMessages
{
    [LoggerMessage(LogLevel.Error, "Capture {Capture} was stored but could not be recorded for the Workbench: {Reason}")]
    internal static partial void RecordingRefused(this ILogger<CaptureRecorder> logger, Guid capture, string reason);

    [LoggerMessage(LogLevel.Error, "Capture {Capture} was stored but recording it for the Workbench failed")]
    internal static partial void RecordingFailed(this ILogger<CaptureRecorder> logger, Guid capture, Exception exception);
}
