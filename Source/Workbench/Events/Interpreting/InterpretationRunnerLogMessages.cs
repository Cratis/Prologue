// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

internal static partial class InterpretationRunnerLogMessages
{
    [LoggerMessage(LogLevel.Error, "Interpreting prologue {Prologue} crashed")]
    internal static partial void Crashed(this ILogger<InterpretationRunner> logger, Guid prologue, Exception exception);

    [LoggerMessage(LogLevel.Error, "Recording the interpretation of prologue {Prologue} did not finish within {Timeout}; Chronicle may be unreachable")]
    internal static partial void RecordingTimedOut(this ILogger<InterpretationRunner> logger, Guid prologue, TimeSpan timeout);
}
