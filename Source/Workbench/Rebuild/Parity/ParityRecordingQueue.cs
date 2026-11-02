// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// Represents an <see cref="IParityRecordingQueue"/> that holds up to <see cref="DefaultCapacity"/> observations in memory. The Extractor
/// only reports a comparison it already made, so a full queue costs that observation and never the Extractor's work. An observation id
/// queued recently is not queued again, so an Extractor retry that arrives before the first report is recorded does not count twice.
/// </summary>
public class ParityRecordingQueue : IParityRecordingQueue
{
    /// <summary>
    /// How many observations wait to be recorded by default.
    /// </summary>
    public const int DefaultCapacity = 5000;

    /// <summary>
    /// How many accepted observation ids are remembered to recognize an Extractor retry.
    /// </summary>
    public const int RememberedIds = 10_000;

    readonly Channel<QueuedParity> _channel;
    readonly HashSet<Guid> _seen = [];
    readonly Queue<Guid> _seenOrder = new();
    readonly Lock _seenLock = new();
    readonly ILogger<ParityRecordingQueue> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParityRecordingQueue"/> class.
    /// </summary>
    /// <param name="logger">The logger a dropped observation is reported to.</param>
    /// <param name="capacity">How many observations may wait.</param>
    public ParityRecordingQueue(ILogger<ParityRecordingQueue> logger, int capacity = DefaultCapacity)
    {
        _logger = logger;
        _channel = Channel.CreateBounded<QueuedParity>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true
        });
    }

    /// <inheritdoc/>
    public bool TryEnqueue(Guid prologue, ParityObservation observation)
    {
        if (!Remember(observation.Id))
        {
            return true;
        }

        if (_channel.Writer.TryWrite(new QueuedParity(prologue, observation)))
        {
            return true;
        }

        Forget(observation.Id);
        _logger.ParityDropped(observation.Id);
        return false;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<QueuedParity> ReadAll(CancellationToken cancellationToken) => _channel.Reader.ReadAllAsync(cancellationToken);

    /// <inheritdoc/>
    public void Forget(Guid observation)
    {
        lock (_seenLock)
        {
            _seen.Remove(observation);
        }
    }

    bool Remember(Guid id)
    {
        lock (_seenLock)
        {
            if (!_seen.Add(id))
            {
                return false;
            }

            _seenOrder.Enqueue(id);
            while (_seenOrder.Count > RememberedIds)
            {
                _seen.Remove(_seenOrder.Dequeue());
            }

            return true;
        }
    }
}
