// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.Recording;
using Cratis.Prologue.Workbench.Rebuild.Parity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Rebuild.for_ParityRecording;

public class when_one_recording_never_completes : Specification
{
    readonly List<Guid> _recorded = [];

    ParityObservation _first;
    ParityObservation _second;
    bool _workerStillRunning;

    async Task Because()
    {
        var prologue = Guid.NewGuid();
        _first = Observation();
        _second = Observation();
        var queue = new ParityRecordingQueue(Microsoft.Extensions.Logging.Abstractions.NullLogger<ParityRecordingQueue>.Instance);
        queue.TryEnqueue(prologue, _first);
        queue.TryEnqueue(prologue, _second);

        var recorder = Substitute.For<IParityRecorder>();
        recorder.Record(Arg.Any<Guid>(), Arg.Any<ParityObservation>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var observation = call.Arg<ParityObservation>();
            if (observation.Id == _first.Id)
            {
                return new TaskCompletionSource().Task;
            }

            _recorded.Add(observation.Id);
            return Task.CompletedTask;
        });

        var services = new ServiceCollection().AddScoped(_ => recorder).BuildServiceProvider();
        var options = Options.Create(new RecordingOptions { ItemTimeout = TimeSpan.FromMilliseconds(100) });
        var worker = new ParityRecordingWorker(queue, services.GetRequiredService<IServiceScopeFactory>(), options, Microsoft.Extensions.Logging.Abstractions.NullLogger<ParityRecordingWorker>.Instance);
        await worker.StartAsync(CancellationToken.None);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (_recorded.Count == 0 && !timeout.IsCancellationRequested)
        {
            await Task.Delay(10);
        }

        _workerStillRunning = worker.ExecuteTask is { IsCompleted: false };
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact] void should_go_on_with_the_next_observation() => _recorded.ShouldContainOnly(_second.Id);
    [Fact] void should_keep_the_worker_running() => _workerStillRunning.ShouldBeTrue();

    static ParityObservation Observation() => new(Guid.NewGuid(), DateTimeOffset.UtcNow, "POST", "/api/loans", "POST /api/loans", "trace", 200, 200, ParityOutcome.Matched);
}
#endif
