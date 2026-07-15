// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents the background service that drains the <see cref="CaptureBuffer"/> and writes batches to the
/// configured <see cref="ICaptureOutput"/>, so capture is never blocked by output I/O.
/// </summary>
/// <param name="buffer">The buffer to drain.</param>
/// <param name="output">The configured output to write to.</param>
/// <param name="logger">The logger.</param>
public class CaptureOutputWorker(CaptureBuffer buffer, ICaptureOutput output, ILogger<CaptureOutputWorker> logger) : BackgroundService
{
    const int MaxBatchSize = 500;

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        CaptureOutputWorkerLog.Started(logger);

        try
        {
            while (await buffer.Reader.WaitToReadAsync(stoppingToken))
            {
                var batch = new List<Capture>();
                while (batch.Count < MaxBatchSize && buffer.Reader.TryRead(out var capture))
                {
                    batch.Add(capture);
                }

                if (batch.Count == 0)
                {
                    continue;
                }

                try
                {
                    await output.Write(batch, stoppingToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    CaptureOutputWorkerLog.WriteFailed(logger, batch.Count, exception);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
