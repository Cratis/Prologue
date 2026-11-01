// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Prologues;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationWorker;

public class when_an_interpretation_throws : Specification
{
    InterpretationProgressStore _progress;
    PrologueId _first;
    PrologueId _second;
    Exception? _escaped;
    bool _workerStillRunning;
    int _runs;

    async Task Because()
    {
        _progress = new InterpretationProgressStore();
        var queue = new InterpretationQueue(_progress);
        _first = PrologueId.New();
        _second = PrologueId.New();
        queue.Enqueue(new InterpretationJob(_first, false));
        queue.Enqueue(new InterpretationJob(_second, false));

        var runner = Substitute.For<IInterpretationRunner>();
        runner.Run(Arg.Any<InterpretationJob>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            _runs++;
            return _runs == 1 ? throw new InvalidOperationException("boom") : Task.CompletedTask;
        });

        var services = new ServiceCollection().AddSingleton(runner).BuildServiceProvider();
        var worker = new InterpretationWorker(queue, services.GetRequiredService<IServiceScopeFactory>(), _progress, Microsoft.Extensions.Logging.Abstractions.NullLogger<InterpretationWorker>.Instance);
        await worker.StartAsync(CancellationToken.None);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (_runs < 2 && !timeout.IsCancellationRequested)
        {
            await Task.Delay(10);
        }

        _escaped = worker.ExecuteTask?.Exception;
        _workerStillRunning = worker.ExecuteTask is { IsCompleted: false };
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact] void should_not_let_the_exception_escape_the_worker() => _escaped.ShouldBeNull();
    [Fact] void should_keep_the_worker_running() => _workerStillRunning.ShouldBeTrue();
    [Fact] void should_show_the_interpretation_as_failed() => _progress.Get(_first).State.ShouldEqual(InterpretationState.Failed);
    [Fact] void should_go_on_with_the_next_interpretation() => _runs.ShouldEqual(2);
}
#endif
