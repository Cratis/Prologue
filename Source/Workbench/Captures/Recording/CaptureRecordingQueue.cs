// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Captures.Recording;

/// <summary>
/// Represents an <see cref="ICaptureRecordingQueue"/> that holds up to <see cref="DefaultCapacity"/> captures in memory. The captures
/// are already stored, so a queue that is full or lost on a restart only costs their summaries in the Workbench.
/// </summary>
public class CaptureRecordingQueue : ICaptureRecordingQueue
{
    /// <summary>
    /// How many captures wait to be recorded by default.
    /// </summary>
    public const int DefaultCapacity = 5000;

    readonly Channel<Capture> _channel;
    readonly ILogger<CaptureRecordingQueue> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CaptureRecordingQueue"/> class.
    /// </summary>
    /// <param name="logger">The logger a dropped recording is reported to.</param>
    /// <param name="capacity">How many captures may wait.</param>
    public CaptureRecordingQueue(ILogger<CaptureRecordingQueue> logger, int capacity = DefaultCapacity)
    {
        _logger = logger;
        _channel = Channel.CreateBounded<Capture>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true
        });
    }

    /// <inheritdoc/>
    public bool TryEnqueue(Capture capture)
    {
        if (_channel.Writer.TryWrite(capture))
        {
            return true;
        }

        _logger.RecordingDropped(capture.Id);
        return false;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<Capture> ReadAll(CancellationToken cancellationToken) => _channel.Reader.ReadAllAsync(cancellationToken);
}
