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
/// <param name="maximumQueuedBytes">The most request-body bytes that may wait in the queue; a request past it is reported as skipped.</param>
public class ShadowDispatcher(IShadowSender sender, IParityReporter reporter, ILogger<ShadowDispatcher> logger, long maximumQueuedBytes) : BackgroundService, IShadowQueue
{
    readonly Channel<ShadowJob> _queue = Channel.CreateBounded<ShadowJob>(new BoundedChannelOptions(ShadowLimits.QueueCapacity)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = false
    });

    long _queuedBytes;

    /// <summary>
    /// Gets the request-body bytes waiting in the queue.
    /// </summary>
    public long QueuedBytes => Interlocked.Read(ref _queuedBytes);

    /// <inheritdoc/>
    public bool TryEnqueue(ShadowJob job)
    {
        var bytes = (long)(job.Body?.Length ?? 0);
        if (bytes > 0 && Interlocked.Add(ref _queuedBytes, bytes) > maximumQueuedBytes)
        {
            Interlocked.Add(ref _queuedBytes, -bytes);
            ShadowingLog.QueueBytesExceeded(logger, job.Rule.Label);
            bytes = 0;
            job = job with { Body = null, Headers = [], BodySkipped = true };
        }

        if (_queue.Writer.TryWrite(job))
        {
            return true;
        }

        Interlocked.Add(ref _queuedBytes, -bytes);
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
                finally
                {
                    Interlocked.Add(ref _queuedBytes, -(job.Body?.Length ?? 0));
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
