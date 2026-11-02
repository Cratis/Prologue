// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Discover.Running;

/// <summary>
/// Log messages for <see cref="AnalysisWorker"/>.
/// </summary>
internal static partial class AnalysisWorkerLogMessages
{
    [LoggerMessage(LogLevel.Error, "Analyzing {Subject} failed unexpectedly")]
    internal static partial void AnalysisCrashed(this ILogger<AnalysisWorker> logger, string subject, Exception exception);

    [LoggerMessage(LogLevel.Error, "Recording the analysis of {Subject} failed unexpectedly")]
    internal static partial void RecordingFailed(this ILogger<AnalysisWorker> logger, string subject, Exception exception);

    [LoggerMessage(LogLevel.Error, "Recording the analysis of {Subject} did not finish within {Timeout}; Chronicle may be unreachable")]
    internal static partial void RecordingTimedOut(this ILogger<AnalysisWorker> logger, string subject, TimeSpan timeout);
}
