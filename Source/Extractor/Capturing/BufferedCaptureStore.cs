// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents an <see cref="ICaptureStore"/> that enqueues captures into the <see cref="CaptureBuffer"/> for a
/// background worker to write, keeping output I/O off the capture path.
/// </summary>
/// <param name="buffer">The buffer captures are enqueued into.</param>
/// <param name="logger">The logger.</param>
public class BufferedCaptureStore(CaptureBuffer buffer, ILogger<BufferedCaptureStore> logger) : ICaptureStore
{
    /// <inheritdoc/>
    public Task Store(Capture capture, CancellationToken cancellationToken = default)
    {
        if (!buffer.TryEnqueue(capture))
        {
            BufferedCaptureStoreLog.CaptureDropped(logger, capture.Id);
        }

        return Task.CompletedTask;
    }
}
