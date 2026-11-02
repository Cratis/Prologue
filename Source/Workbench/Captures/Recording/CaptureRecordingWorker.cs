// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common.Recording;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Captures.Recording;

/// <summary>
/// Records the queued captures one at a time in the background, each in a scope of its own. One capture that cannot be recorded
/// is logged and the next is recorded; nothing here can reach the request that stored the capture. A capture that takes longer than
/// <see cref="RecordingOptions.ItemTimeout"/> (Chronicle unreachable) is logged and given up on, so the queue keeps moving; it stays stored but is not recorded.
/// </summary>
/// <param name="queue">The queue to take captures from.</param>
/// <param name="scopes">Creates the scope a recording runs in.</param>
/// <param name="options">The recording options, for the time one capture may take.</param>
/// <param name="logger">The logger.</param>
public class CaptureRecordingWorker(ICaptureRecordingQueue queue, IServiceScopeFactory scopes, IOptions<RecordingOptions> options, ILogger<CaptureRecordingWorker> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var capture in queue.ReadAll(stoppingToken))
            {
                try
                {
                    var finished = await BoundedRecording.Run(
                        scopes,
                        options.Value.ItemTimeout,
                        services => services.GetRequiredService<ICaptureRecorder>().Record(capture, stoppingToken),
                        stoppingToken);
                    if (!finished)
                    {
                        logger.RecordingTimedOut(capture.Id, options.Value.ItemTimeout);
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.RecordingCrashed(capture.Id, exception);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
