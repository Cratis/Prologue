// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover.Analyzing;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discover.Running.for_AnalysisProgressStore;

public class when_progress_is_set_concurrently : Specification
{
    const int Analyses = 16;
    const int Updates = 200;

    AnalysisProgressStore _store;
    int _overlapping;
    int _inside;
    IReadOnlyList<AnalysisProgress> _last;

    static AnalysisProgress Progress(int analysis, int filesRead)
    {
        var folder = $"/srv/folder-{analysis}";
        var target = new AnalysisTarget(AnalysisSubjectId.ForFolder(folder), new SubjectName(folder), folder, folder, RepositoryHost.LocalFolder, new RepositoryName(folder), new BranchName(string.Empty), new SubPath(string.Empty));
        return new AnalysisProgress(PrologueId.New(), target, AnalysisState.Running, filesRead, [], Updates, null);
    }

    void Because()
    {
        _store = new AnalysisProgressStore();
        using var subscription = _store.Observe().Subscribe(_ =>
        {
            if (Interlocked.Increment(ref _inside) > 1)
            {
                Interlocked.Increment(ref _overlapping);
            }

            Thread.SpinWait(200);
            Interlocked.Decrement(ref _inside);
        });

        Parallel.For(0, Analyses, analysis =>
        {
            for (var read = 1; read <= Updates; read++)
            {
                _store.Set(Progress(analysis, read));
            }
        });

        using var latest = _store.Observe().Subscribe(all => _last = all);
    }

    [Fact] void should_tell_observers_one_change_at_a_time() => _overlapping.ShouldEqual(0);
    [Fact] void should_end_with_every_analysis_at_its_last_update() => _last.Select(progress => progress.FilesRead).Distinct().ShouldContainOnly(Updates);
    [Fact] void should_hold_every_analysis() => _last.Count.ShouldEqual(Analyses);
}
#endif
