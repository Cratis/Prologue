// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Log messages for <see cref="ServiceLifecycleWatcher"/>.
/// </summary>
internal static partial class ServiceLifecycleWatcherLog
{
    [LoggerMessage(LogLevel.Information, "No answers arrived within the grace period of {GracePeriod}; exiting cleanly so the orchestrator can resume the session later")]
    internal static partial void GracePeriodExpired(ILogger logger, TimeSpan gracePeriod);

    [LoggerMessage(LogLevel.Information, "The session finished, its result was fetched and nothing happened within {IdleTimeout}; exiting cleanly")]
    internal static partial void IdleAfterCompletion(ILogger logger, TimeSpan idleTimeout);

    [LoggerMessage(LogLevel.Information, "No session activity happened within {IdleTimeout}; exiting cleanly")]
    internal static partial void NoActivity(ILogger logger, TimeSpan idleTimeout);
}
