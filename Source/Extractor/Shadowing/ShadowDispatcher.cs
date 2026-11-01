// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Channels;

namespace Cratis.Prologue.Extractor.Shadowing;

/// <summary>
/// Mirrors queued requests to the shadow destination with bounded concurrency, compares the answers with the legacy system's and
/// reports parity. It works after the caller has its response and never throws into the request pipeline.
/// </summary>
/// <param name="sender">Sends the copies.</param>
/// <param name="reporter">Reports the observations.</param>
/// <param name="logger">The logger.</param>
public class ShadowDispatcher(IShadowSender sender, IParityReporter reporter, ILogger<ShadowDispatcher> logger) : BackgroundService, IShadowQueue
{
    readonly Channel<ShadowJob> _queue = Channel.CreateBounded<ShadowJob>(new BoundedChannelOptions(ShadowLimits.QueueCapacity)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = false
    });

    /// <inheritdoc/>
    public bool TryEnqueue(ShadowJob job)
    {
        if (_queue.Writer.TryWrite(job))
        {
            return true;
        }

        ShadowingLog.QueueFull(logger, job.Rule.Label);
        return false;
    }

    /// <summary>
    /// Mirrors one request and reports the result.
    /// </summary>
    /// <param name="job">The request.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Process(ShadowJob job, CancellationToken cancellationToken)
    {
        var outcome = ParityOutcome.Skipped;
        int? shadowStatus = null;
        if (!job.BodySkipped)
        {
            shadowStatus = await sender.Send(job, cancellationToken);
            outcome = ShadowComparison.Compare(job.LegacyStatus, shadowStatus);
        }

        await reporter.Report(
            new ParityObservation(job.Id, job.Occurred, job.Method, job.Path, job.Shape, job.Trace, job.LegacyStatus, shadowStatus ?? 0, outcome),
            cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workers = Enumerable.Range(0, ShadowLimits.MaximumConcurrency).Select(_ => Work(stoppingToken));
        await Task.WhenAll(workers);
    }

    async Task Work(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await Process(job, stoppingToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    ShadowingLog.ShadowingFailed(logger, job.Rule.Label, exception);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
