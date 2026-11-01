// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.Captures.Recording.for_CaptureRecordingQueue;

public class when_the_queue_is_full : Specification
{
    ILogger<CaptureRecordingQueue> _logger;
    CaptureRecordingQueue _queue;
    bool _first;
    bool _second;
    bool _third;
    List<Guid> _read;

    static Capture Capture() => new(Guid.NewGuid(), DateTimeOffset.UtcNow, []);

    async Task Because()
    {
        _logger = Substitute.For<ILogger<CaptureRecordingQueue>>();
        _logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _queue = new CaptureRecordingQueue(_logger, 2);
        var one = Capture();
        var two = Capture();
        _first = _queue.TryEnqueue(one);
        _second = _queue.TryEnqueue(two);
        _third = _queue.TryEnqueue(Capture());

        _read = [];
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        try
        {
            await foreach (var capture in _queue.ReadAll(cancellation.Token))
            {
                _read.Add(capture.Id);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Fact] void should_take_captures_up_to_the_capacity() => (_first && _second).ShouldBeTrue();
    [Fact] void should_drop_instead_of_waiting_when_full() => _third.ShouldBeFalse();
    [Fact] void should_say_a_recording_was_dropped() => _logger.ReceivedCalls().Count(call => call.GetMethodInfo().Name == nameof(ILogger.Log)).ShouldEqual(1);
    [Fact] void should_hand_out_what_it_kept_oldest_first() => _read.Count.ShouldEqual(2);
}
#endif
