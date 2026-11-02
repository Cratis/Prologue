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
/// recorded is logged and the next is recorded; nothing here can reach the request that received the observation. An observation that takes longer than
/// <see cref="RecordingOptions.ItemTimeout"/> (Chronicle unreachable) is logged and given up on, so the queue keeps moving; it is not recorded.
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
                try
                {
                    var finished = await BoundedRecording.Run(
                        scopes,
                        options.Value.ItemTimeout,
                        services => services.GetRequiredService<IParityRecorder>().Record(queued.Prologue, queued.Observation, stoppingToken),
                        stoppingToken);
                    if (!finished)
                    {
                        logger.ParityTimedOut(queued.Observation.Id, options.Value.ItemTimeout);
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.ParityCrashed(queued.Observation.Id, exception);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
