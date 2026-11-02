// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Rebuild.Parity;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Prologue.Workbench.Rebuild.for_ParityRecording;

public class when_one_recording_throws : Specification
{
    readonly List<(Guid Prologue, Guid Observation)> _recorded = [];

    Guid _prologue;
    ParityObservation _first;
    ParityObservation _second;
    Exception? _escaped;
    bool _workerStillRunning;

    async Task Because()
    {
        _prologue = Guid.NewGuid();
        _first = Observation();
        _second = Observation();
        var queue = new ParityRecordingQueue(Microsoft.Extensions.Logging.Abstractions.NullLogger<ParityRecordingQueue>.Instance);
        queue.TryEnqueue(_prologue, _first);
        queue.TryEnqueue(_prologue, _second);

        var recorder = Substitute.For<IParityRecorder>();
        recorder.Record(Arg.Any<Guid>(), Arg.Any<ParityObservation>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var observation = call.Arg<ParityObservation>();
            if (observation.Id == _first.Id)
            {
                throw new InvalidOperationException("boom");
            }

            _recorded.Add((call.Arg<Guid>(), observation.Id));
            return Task.CompletedTask;
        });

        var services = new ServiceCollection().AddScoped(_ => recorder).BuildServiceProvider();
        var worker = new ParityRecordingWorker(queue, services.GetRequiredService<IServiceScopeFactory>(), Microsoft.Extensions.Logging.Abstractions.NullLogger<ParityRecordingWorker>.Instance);
        await worker.StartAsync(CancellationToken.None);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (_recorded.Count == 0 && !timeout.IsCancellationRequested)
        {
            await Task.Delay(10);
        }

        _escaped = worker.ExecuteTask?.Exception;
        _workerStillRunning = worker.ExecuteTask is { IsCompleted: false };
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact] void should_not_let_the_exception_escape_the_worker() => _escaped.ShouldBeNull();
    [Fact] void should_keep_the_worker_running() => _workerStillRunning.ShouldBeTrue();
    [Fact] void should_go_on_with_the_next_observation() => _recorded.ShouldContainOnly((_prologue, _second.Id));

    static ParityObservation Observation() => new(Guid.NewGuid(), DateTimeOffset.UtcNow, "POST", "/api/loans", "POST /api/loans", "trace", 200, 200, ParityOutcome.Matched);
}
#endif
