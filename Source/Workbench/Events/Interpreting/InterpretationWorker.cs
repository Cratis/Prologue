// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Runs the queued interpretations one at a time in the background, each in a scope of its own.
/// </summary>
/// <param name="queue">The queue to take jobs from.</param>
/// <param name="scopes">Creates the scope a job runs in.</param>
/// <param name="progress">Where an interpretation that failed unexpectedly is shown as failed.</param>
/// <param name="logger">The logger.</param>
public class InterpretationWorker(IInterpretationQueue queue, IServiceScopeFactory scopes, IInterpretationProgress progress, ILogger<InterpretationWorker> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            InterpretationJob job;
            try
            {
                job = await queue.Next(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IInterpretationRunner>().Run(job, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                // One interpretation that fails must not stop the worker, or the host, and every later interpretation with it.
                logger.InterpretationCrashed(job.Prologue.Value, exception);
                progress.Set(new InterpretationProgress(job.Prologue, InterpretationState.Failed, "The interpretation could not finish", false, 0));
            }
            finally
            {
                queue.Done(job.Prologue);
            }
        }
    }
}
