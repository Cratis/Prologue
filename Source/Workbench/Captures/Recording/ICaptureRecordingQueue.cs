// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures.Recording;

/// <summary>
/// Holds stored captures that wait to be recorded for the Workbench, so the endpoint that stored one can answer without waiting for
/// the event store.
/// </summary>
public interface ICaptureRecordingQueue
{
    /// <summary>
    /// Queues a stored capture to be recorded. When the queue is full the recording is dropped and logged; the stored capture is kept.
    /// </summary>
    /// <param name="capture">The stored capture.</param>
    /// <returns>True when it was queued; false when it was dropped.</returns>
    bool TryEnqueue(Capture capture);

    /// <summary>
    /// Reads the queued captures as they arrive, until cancelled.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that ends the reading.</param>
    /// <returns>The captures, oldest first.</returns>
    IAsyncEnumerable<Capture> ReadAll(CancellationToken cancellationToken);
}
