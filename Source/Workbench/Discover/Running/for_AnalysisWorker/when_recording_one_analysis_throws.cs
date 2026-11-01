// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Commands;
using Cratis.Execution;
using Cratis.Prologue.Workbench.Discover.Analyzing;
using Cratis.Prologue.Workbench.Discover.Recording;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discover.Running.for_AnalysisWorker;

public class when_recording_one_analysis_throws : Specification
{
    AnalysisProgressStore _progress;
    AnalysisJob _first;
    AnalysisJob _second;
    int _recorded;
    Exception? _escaped;
    bool _workerStillRunning;

    static AnalysisJob Job(string folder) => new(
        PrologueId.New(),
        new AnalysisTarget(AnalysisSubjectId.ForFolder(folder), new SubjectName(folder), folder, folder, RepositoryHost.LocalFolder, new RepositoryName(folder), new BranchName(string.Empty), new SubPath(string.Empty)));

    async Task Because()
    {
        _progress = new AnalysisProgressStore();
        var queue = new AnalysisQueue(_progress);
        _first = Job("/srv/first");
        _second = Job("/srv/second");
        queue.Enqueue(_first);
        queue.Enqueue(_second);

        var runner = Substitute.For<IAnalysisRunner>();
        runner.Run(Arg.Any<AnalysisJob>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var job = call.Arg<AnalysisJob>();
            return new AnalysisOutcome(job.Target.Subject, job.Target.Name, job.Target.Source, new FileCount(1), AnalysisFindings.Empty, null);
        });

        var pipeline = Substitute.For<ICommandPipeline>();
        pipeline.Execute(Arg.Any<object>(), Arg.Any<ValidationResultSeverity?>()).Returns<Task<CommandResult>>(call =>
        {
            _recorded++;
            return _recorded == 1 ? throw new InvalidOperationException("the event store is unavailable") : Task.FromResult(CommandResult.Success(CorrelationId.New()));
        });

        var worker = new AnalysisWorker(queue, runner, _progress, pipeline, Microsoft.Extensions.Logging.Abstractions.NullLogger<AnalysisWorker>.Instance);
        await worker.StartAsync(CancellationToken.None);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (_progress.Get(_second.Target.Subject)?.State != AnalysisState.Complete && !timeout.IsCancellationRequested)
        {
            await Task.Delay(10);
        }

        _escaped = worker.ExecuteTask?.Exception;
        _workerStillRunning = worker.ExecuteTask is { IsCompleted: false };
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact] void should_not_let_the_exception_escape_the_worker() => _escaped.ShouldBeNull();
    [Fact] void should_keep_the_worker_running() => _workerStillRunning.ShouldBeTrue();
    [Fact] void should_mark_the_analysis_that_failed_as_failed() => _progress.Get(_first.Target.Subject)!.State.ShouldEqual(AnalysisState.Failed);
    [Fact] void should_say_why() => _progress.Get(_first.Target.Subject)!.Reason!.Value.ShouldContain("event store");
    [Fact] void should_go_on_with_the_next_analysis() => _progress.Get(_second.Target.Subject)!.State.ShouldEqual(AnalysisState.Complete);
}
#endif
