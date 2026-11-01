// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Log messages for <see cref="InterpretationWorker"/>.
/// </summary>
internal static partial class InterpretationWorkerLogMessages
{
    [LoggerMessage(LogLevel.Error, "Interpreting prologue {Prologue} failed unexpectedly")]
    internal static partial void InterpretationCrashed(this ILogger<InterpretationWorker> logger, Guid prologue, Exception exception);
}
