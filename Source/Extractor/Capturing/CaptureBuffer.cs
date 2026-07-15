// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Channels;

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents the in-process buffer between the capture pipeline and the output writer. Enqueue is non-blocking so
/// output I/O never adds latency to capture — like an async logging sink. When the buffer is full the oldest
/// captures are dropped in favor of the most recent.
/// </summary>
public class CaptureBuffer
{
    /// <summary>
    /// The maximum number of captures held in the buffer before the oldest are dropped.
    /// </summary>
    public const int Capacity = 100_000;

    readonly Channel<Capture> _channel = Channel.CreateBounded<Capture>(new BoundedChannelOptions(Capacity)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleReader = true,
        SingleWriter = false,
    });

    /// <summary>
    /// Gets the reader the output worker drains.
    /// </summary>
    public ChannelReader<Capture> Reader => _channel.Reader;

    /// <summary>
    /// Enqueues a capture without blocking.
    /// </summary>
    /// <param name="capture">The capture to enqueue.</param>
    /// <returns>True if the capture was accepted; false if it was dropped.</returns>
    public bool TryEnqueue(Capture capture) => _channel.Writer.TryWrite(capture);
}
