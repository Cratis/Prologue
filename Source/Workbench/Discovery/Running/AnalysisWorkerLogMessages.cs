// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Discovery.Running;

/// <summary>
/// Log messages for <see cref="AnalysisWorker"/>.
/// </summary>
internal static partial class AnalysisWorkerLogMessages
{
    [LoggerMessage(LogLevel.Error, "Analyzing {Subject} failed unexpectedly")]
    internal static partial void AnalysisCrashed(this ILogger<AnalysisWorker> logger, string subject, Exception exception);
}
