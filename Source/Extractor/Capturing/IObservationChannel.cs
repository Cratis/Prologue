// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Defines the in-process producer/consumer boundary that configured observation producers publish to and the
/// correlation worker consumes from. Source hosting, payload registration, persistence, and interpretation are
/// composed separately; this interface alone is not an external plug-in contract. It is the narrow seam through
/// which a configured producer enters the capture pipeline.
/// </summary>
public interface IObservationChannel
{
    /// <summary>
    /// Publishes an observation for correlation.
    /// </summary>
    /// <param name="observation">The <see cref="Observation"/> to publish.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Publish(Observation observation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all published observations as they arrive.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that completes the stream when cancelled.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of observations.</returns>
    IAsyncEnumerable<Observation> ReadAll(CancellationToken cancellationToken);
}
