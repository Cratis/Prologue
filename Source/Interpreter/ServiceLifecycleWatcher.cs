// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Watches the session activity and exits the service cleanly when a session has been awaiting answers beyond the
/// grace period, when a finished session's fetched result has gone idle, or when no session activity happens at
/// all. The session state is persisted at every checkpoint, so exiting is always safe — the orchestrator detects
/// the container is gone and restarts it later to resume.
/// </summary>
/// <param name="arguments">The <see cref="InterpreterArguments"/> carrying the grace period and idle timeout.</param>
/// <param name="activity">The <see cref="ISessionActivityTracker"/> the decision is made from.</param>
/// <param name="lifetime">The <see cref="IHostApplicationLifetime"/> used to stop the host.</param>
/// <param name="timeProvider">The <see cref="TimeProvider"/> driving the periodic evaluation.</param>
/// <param name="logger">The logger.</param>
public class ServiceLifecycleWatcher(
    InterpreterArguments arguments,
    ISessionActivityTracker activity,
    IHostApplicationLifetime lifetime,
    TimeProvider timeProvider,
    ILogger<ServiceLifecycleWatcher> logger) : BackgroundService
{
    static readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(1);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var timer = new PeriodicTimer(_checkInterval, timeProvider);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (EvaluateOnce())
                {
                    return;
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The host is stopping for another reason — there is no lifecycle decision left to make.
        }
    }

    bool EvaluateOnce()
    {
        switch (activity.Evaluate(arguments.GracePeriod, arguments.IdleTimeout))
        {
            case ShutdownReason.GracePeriodExpired:
                ServiceLifecycleWatcherLog.GracePeriodExpired(logger, arguments.GracePeriod);
                break;
            case ShutdownReason.IdleAfterCompletion:
                ServiceLifecycleWatcherLog.IdleAfterCompletion(logger, arguments.IdleTimeout);
                break;
            case ShutdownReason.NoActivity:
                ServiceLifecycleWatcherLog.NoActivity(logger, arguments.IdleTimeout);
                break;
            default:
                return false;
        }

        lifetime.StopApplication();
        return true;
    }
}
