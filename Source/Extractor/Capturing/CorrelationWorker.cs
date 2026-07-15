// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents the background service that drains observations from the channel into the correlator, then
/// periodically flushes settled captures to the store.
/// </summary>
/// <param name="channel">The channel observations are published to.</param>
/// <param name="correlator">The correlator that groups observations into captures.</param>
/// <param name="store">The store settled captures are persisted to.</param>
/// <param name="options">The Prologue options carrying the correlation window.</param>
/// <param name="logger">The logger.</param>
public class CorrelationWorker(
    IObservationChannel channel,
    ICorrelator correlator,
    ICaptureStore store,
    IOptions<PrologueOptions> options,
    ILogger<CorrelationWorker> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        CorrelationWorkerLog.Started(logger);
        await Task.WhenAll(Ingest(stoppingToken), Flush(stoppingToken));
    }

    async Task Ingest(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var observation in channel.ReadAll(stoppingToken))
            {
                correlator.Add(observation);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    async Task Flush(CancellationToken stoppingToken)
    {
        var window = options.Value.Correlation.Window;
        var interval = window < TimeSpan.FromMilliseconds(400) ? TimeSpan.FromMilliseconds(100) : window / 4;
        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                foreach (var capture in correlator.Drain(DateTimeOffset.UtcNow))
                {
                    await store.Store(capture, stoppingToken);
                    CorrelationWorkerLog.CaptureStored(logger, capture.Id, capture.Entries.Count);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
