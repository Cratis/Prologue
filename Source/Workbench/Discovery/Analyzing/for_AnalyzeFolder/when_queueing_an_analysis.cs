// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discovery.Running;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discovery.Analyzing.for_AnalyzeFolder;

public class when_queueing_an_analysis : a_prologue
{
    IAnalysisProgress _progress;
    AnalysisQueue _queue;
    AnalysisJob _job;
    bool _first;
    bool _second;
    AnalysisJob _next;
    bool _afterDone;

    void Establish()
    {
        _progress = new AnalysisProgressStore();
        _queue = new AnalysisQueue(_progress);
        var target = new AnalysisTarget(AnalysisSubjectId.ForFolder("/srv/legacy"), new SubjectName("legacy"), "/srv/legacy", "/srv/legacy", RepositoryHost.LocalFolder, new RepositoryName("/srv/legacy"), new BranchName(string.Empty), new SubPath(string.Empty));
        _job = new AnalysisJob(_id, target);
    }

    async Task Because()
    {
        new AnalyzeFolder(_id, new FolderPath("/srv/legacy")).Handle(_job.Target, _queue);
        _first = true;
        _second = _queue.Enqueue(_job);
        _next = await _queue.Next(CancellationToken.None);
        _queue.Done(_next);
        _afterDone = _queue.Enqueue(_job);
    }

    [Fact] void should_have_queued_the_job() => _first.ShouldBeTrue();
    [Fact] void should_not_queue_the_same_folder_twice() => _second.ShouldBeFalse();
    [Fact] void should_hand_out_the_job() => _next.ShouldEqual(_job);
    [Fact] void should_queue_the_folder_again_once_it_is_done() => _afterDone.ShouldBeTrue();
    [Fact] void should_show_the_job_as_waiting() => _progress.Get(_job.Target.Subject)!.State.ShouldEqual(AnalysisState.Queued);
}
#endif
