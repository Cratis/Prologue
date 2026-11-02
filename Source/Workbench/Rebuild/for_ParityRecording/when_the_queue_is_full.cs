// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Rebuild.Parity;

namespace Cratis.Prologue.Workbench.Rebuild.for_ParityRecording;

public class when_the_queue_is_full : Specification
{
    bool _first;
    bool _second;
    bool _third;
    List<Guid> _read = [];

    async Task Because()
    {
        var queue = new ParityRecordingQueue(Microsoft.Extensions.Logging.Abstractions.NullLogger<ParityRecordingQueue>.Instance, 2);
        _first = queue.TryEnqueue(Guid.Empty, Observation(1));
        _second = queue.TryEnqueue(Guid.Empty, Observation(2));
        _third = queue.TryEnqueue(Guid.Empty, Observation(3));

        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        try
        {
            await foreach (var queued in queue.ReadAll(timeout.Token))
            {
                _read.Add(queued.Observation.Id);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Fact] void should_queue_up_to_the_capacity() => (_first && _second).ShouldBeTrue();
    [Fact] void should_drop_the_one_that_does_not_fit() => _third.ShouldBeFalse();
    [Fact] void should_read_the_queued_ones_oldest_first() => _read.ShouldEqual([Id(1), Id(2)]);

    static Guid Id(int n) => new(n, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    static ParityObservation Observation(int n) => new(Id(n), DateTimeOffset.UtcNow, "POST", "/api/loans", "POST /api/loans", "trace", 200, 200, ParityOutcome.Matched);
}
#endif
