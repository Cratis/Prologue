// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Channels;

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents an unbounded, single-reader, multi-writer implementation of <see cref="IObservationChannel"/>
/// backed by <see cref="Channel{T}"/>.
/// </summary>
public class ObservationChannel : IObservationChannel
{
    readonly Channel<Observation> _channel = Channel.CreateUnbounded<Observation>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    /// <inheritdoc/>
    public ValueTask Publish(Observation observation, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(observation, cancellationToken);

    /// <inheritdoc/>
    public IAsyncEnumerable<Observation> ReadAll(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
