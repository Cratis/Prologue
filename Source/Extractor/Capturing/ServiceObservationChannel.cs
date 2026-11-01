// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents an <see cref="IObservationChannel"/> that attributes every observation published through it to one
/// service before handing it on, so a source that belongs to a service never has to know about services at all.
/// </summary>
/// <param name="inner">The channel observations are handed on to.</param>
/// <param name="service">The service the observations belong to.</param>
public class ServiceObservationChannel(IObservationChannel inner, ServiceName service) : IObservationChannel
{
    /// <inheritdoc/>
    public ValueTask Publish(Observation observation, CancellationToken cancellationToken = default) =>
        inner.Publish(observation with { Service = service }, cancellationToken);

    /// <inheritdoc/>
    public IAsyncEnumerable<Observation> ReadAll(CancellationToken cancellationToken) => inner.ReadAll(cancellationToken);
}
