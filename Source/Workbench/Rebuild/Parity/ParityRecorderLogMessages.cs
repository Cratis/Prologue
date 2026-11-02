// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// Log messages for <see cref="ParityRecorder"/>, <see cref="ParityRecordingQueue"/> and <see cref="ParityRecordingWorker"/>.
/// </summary>
internal static partial class ParityRecorderLogMessages
{
    [LoggerMessage(LogLevel.Warning, "The parity observation {Observation} was refused: {Reason}")]
    internal static partial void ParityRefused(this ILogger<ParityRecorder> logger, Guid observation, string reason);

    [LoggerMessage(LogLevel.Warning, "The parity observation {Observation} could not be recorded")]
    internal static partial void ParityFailed(this ILogger<ParityRecorder> logger, Guid observation, Exception exception);

    [LoggerMessage(LogLevel.Error, "The parity recording queue is full; observation {Observation} is not recorded")]
    internal static partial void ParityDropped(this ILogger<ParityRecordingQueue> logger, Guid observation);

    [LoggerMessage(LogLevel.Error, "Recording parity observation {Observation} failed unexpectedly")]
    internal static partial void ParityCrashed(this ILogger<ParityRecordingWorker> logger, Guid observation, Exception exception);

    [LoggerMessage(LogLevel.Error, "Recording parity observation {Observation} did not finish within {Timeout}; Chronicle may be unreachable. The observation is not recorded")]
    internal static partial void ParityTimedOut(this ILogger<ParityRecordingWorker> logger, Guid observation, TimeSpan timeout);
}
