// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// Holds parity observations that wait to be recorded, so the endpoint that received one can answer without waiting for the event store.
/// </summary>
public interface IParityRecordingQueue
{
    /// <summary>
    /// Queues an observation to be recorded. When the queue is full the observation is dropped and logged.
    /// </summary>
    /// <param name="prologue">The prologue the Extractor is configured for.</param>
    /// <param name="observation">The observation.</param>
    /// <returns>True when it was queued; false when it was dropped.</returns>
    bool TryEnqueue(Guid prologue, ParityObservation observation);

    /// <summary>
    /// Reads the queued observations as they arrive, until cancelled.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that ends the reading.</param>
    /// <returns>The observations, oldest first.</returns>
    IAsyncEnumerable<QueuedParity> ReadAll(CancellationToken cancellationToken);
}
