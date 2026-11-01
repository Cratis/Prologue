// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// Log messages for <see cref="ParityRecorder"/>.
/// </summary>
internal static partial class ParityRecorderLogMessages
{
    [LoggerMessage(LogLevel.Warning, "The parity observation {Observation} was refused: {Reason}")]
    internal static partial void ParityRefused(this ILogger<ParityRecorder> logger, Guid observation, string reason);

    [LoggerMessage(LogLevel.Warning, "The parity observation {Observation} could not be recorded")]
    internal static partial void ParityFailed(this ILogger<ParityRecorder> logger, Guid observation, Exception exception);
}
