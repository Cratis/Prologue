// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Rebuild.Parity;

namespace Cratis.Prologue.Workbench.Rebuild.for_ParityRecording;

public class when_an_observation_is_queued_twice : Specification
{
    readonly List<Guid> _read = [];

    bool _retry;

    async Task Because()
    {
        var queue = new ParityRecordingQueue(Microsoft.Extensions.Logging.Abstractions.NullLogger<ParityRecordingQueue>.Instance, 2);
        var first = Observation(Guid.NewGuid());
        queue.TryEnqueue(Guid.Empty, first);
        _retry = queue.TryEnqueue(Guid.Empty, first);
        queue.TryEnqueue(Guid.Empty, Observation(Guid.NewGuid()));

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

    static ParityObservation Observation(Guid id) => new(id, DateTimeOffset.UtcNow, "POST", "/api/loans", "POST /api/loans", "trace", 200, 200, ParityOutcome.Matched);

    [Fact] void should_accept_the_retry() => _retry.ShouldBeTrue();
    [Fact] void should_queue_the_observation_once() => _read.Count.ShouldEqual(2);
    [Fact] void should_not_let_the_retry_take_room_in_the_queue() => _read.Distinct().Count().ShouldEqual(2);
}
#endif
