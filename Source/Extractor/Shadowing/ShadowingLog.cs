// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Shadowing;

internal static partial class ShadowingLog
{
    [LoggerMessage(LogLevel.Warning, "The shadow destination {Host} for {Rule} could not be reached or did not answer in time")]
    internal static partial void ShadowUnavailable(ILogger logger, string rule, string host, Exception exception);

    [LoggerMessage(LogLevel.Warning, "Parity observation {Observation} could not be reported to the Receiver")]
    internal static partial void ReportFailed(ILogger logger, Guid observation, Exception exception);

    [LoggerMessage(LogLevel.Warning, "The shadowing queue is full; {Rule} was not mirrored")]
    internal static partial void QueueFull(ILogger logger, string rule);

    [LoggerMessage(LogLevel.Warning, "The shadowing queue holds its limit of request bodies; {Rule} is reported as skipped")]
    internal static partial void QueueBytesExceeded(ILogger logger, string rule);

    [LoggerMessage(LogLevel.Warning, "Shadowing failed for {Rule} without affecting the response")]
    internal static partial void ShadowingFailed(ILogger logger, string rule, Exception exception);

    [LoggerMessage(LogLevel.Information, "Shadowing {Count} commands to the new system; parity is reported for Prologue {Prologue}")]
    internal static partial void Started(ILogger logger, int count, Guid prologue);
}
