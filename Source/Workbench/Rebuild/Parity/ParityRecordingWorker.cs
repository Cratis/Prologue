// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common.Recording;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// Records the queued parity observations one at a time in the background, each in a scope of its own. One observation that cannot be
/// recorded is logged and the next is recorded; nothing here can reach the request that received the observation. An observation that fails or takes longer than
/// <see cref="RecordingOptions.ItemTimeout"/> (Chronicle unreachable) is tried again a bounded number of times, then logged and given up on, so the queue keeps moving.
/// </summary>
/// <param name="queue">The queue to take observations from.</param>
/// <param name="scopes">Creates the scope a recording runs in.</param>
/// <param name="options">The recording options, for the time one observation may take.</param>
/// <param name="logger">The logger.</param>
public class ParityRecordingWorker(IParityRecordingQueue queue, IServiceScopeFactory scopes, IOptions<RecordingOptions> options, ILogger<ParityRecordingWorker> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var queued in queue.ReadAll(stoppingToken))
            {
                await RecordWithRetries(queued, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    // Recording the same observation twice records it once, so a failed or timed-out attempt is safe to repeat. Only a bounded number of attempts
    // is made, with a growing pause, so an event store that stays down cannot hold the queue for long; then the id is forgotten so the Extractor can report it again.
    async Task RecordWithRetries(QueuedParity queued, CancellationToken stoppingToken)
    {
        var attempts = Math.Max(1, options.Value.ParityAttempts);
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            if (await TryRecord(queued, stoppingToken))
            {
                return;
            }

            if (attempt < attempts)
            {
                await Task.Delay(options.Value.ParityBackoff * attempt, stoppingToken);
            }
        }

        queue.Forget(queued.Observation.Id);
        logger.ParityGivenUp(queued.Observation.Id, attempts);
    }

    async Task<bool> TryRecord(QueuedParity queued, CancellationToken stoppingToken)
    {
        try
        {
            var recorded = false;
            var finished = await BoundedRecording.Run(
                scopes,
                options.Value.ItemTimeout,
                async services => recorded = await services.GetRequiredService<IParityRecorder>().Record(queued.Prologue, queued.Observation, stoppingToken),
                stoppingToken);
            if (!finished)
            {
                logger.ParityTimedOut(queued.Observation.Id, options.Value.ItemTimeout);
            }

            return finished && recorded;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.ParityCrashed(queued.Observation.Id, exception);
            return false;
        }
    }
}
