// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Captures.Recording;

/// <summary>
/// Records the queued captures one at a time in the background, each in a scope of its own. One capture that cannot be recorded
/// is logged and the next is recorded; nothing here can reach the request that stored the capture.
/// </summary>
/// <param name="queue">The queue to take captures from.</param>
/// <param name="scopes">Creates the scope a recording runs in.</param>
/// <param name="logger">The logger.</param>
public class CaptureRecordingWorker(ICaptureRecordingQueue queue, IServiceScopeFactory scopes, ILogger<CaptureRecordingWorker> logger) : BackgroundService
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
                    await using var scope = scopes.CreateAsyncScope();
                    await scope.ServiceProvider.GetRequiredService<ICaptureRecorder>().Record(capture, stoppingToken);
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
