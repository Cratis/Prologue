// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Prologue.Workbench.Captures.Recording.for_CaptureRecordingWorker;

public class when_one_recording_throws : Specification
{
    readonly List<Guid> _recorded = [];

    Capture _first;
    Capture _second;
    Exception? _escaped;
    bool _workerStillRunning;

    async Task Because()
    {
        _first = new Capture(Guid.NewGuid(), DateTimeOffset.UtcNow, []);
        _second = new Capture(Guid.NewGuid(), DateTimeOffset.UtcNow, []);
        var queue = new CaptureRecordingQueue(Microsoft.Extensions.Logging.Abstractions.NullLogger<CaptureRecordingQueue>.Instance);
        queue.TryEnqueue(_first);
        queue.TryEnqueue(_second);

        var recorder = Substitute.For<ICaptureRecorder>();
        recorder.Record(Arg.Any<Capture>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var capture = call.Arg<Capture>();
            if (capture.Id == _first.Id)
            {
                throw new InvalidOperationException("boom");
            }

            _recorded.Add(capture.Id);
            return Task.CompletedTask;
        });

        var services = new ServiceCollection().AddScoped(_ => recorder).BuildServiceProvider();
        var worker = new CaptureRecordingWorker(queue, services.GetRequiredService<IServiceScopeFactory>(), Microsoft.Extensions.Logging.Abstractions.NullLogger<CaptureRecordingWorker>.Instance);
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
    [Fact] void should_go_on_with_the_next_capture() => _recorded.ShouldContainOnly(_second.Id);
}
#endif
