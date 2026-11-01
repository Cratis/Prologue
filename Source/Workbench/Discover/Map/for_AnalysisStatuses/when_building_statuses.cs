// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover.Analyzing;
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Discover.given;
using Cratis.Prologue.Workbench.Discover.Running;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;
using Cratis.Prologue.Workbench.Systems.Locating;

namespace Cratis.Prologue.Workbench.Discover.Map.for_AnalysisStatuses;

public class when_building_statuses : a_prologue
{
    List<AnalysisStatus> _statuses;
    List<AnalysisStatus> _whileRunning;

    void Because()
    {
        var located = PrologueDefinition.Reduce(
            new SystemCodeLocated(_lending, RepositoryHost.GitHub, new RepositoryName("cratis/library-platform"), new BranchName("main"), new SubPath("services/core")),
            _prologue);

        var lendingRun = new CodeAnalysisCompleted(AnalysisSubjectId.For(_lending), new SubjectName("Lending"), new AnalysisSource("a41c9e2"), new FileCount(1412));
        var folderRun = new CodeAnalysisFailed(AnalysisSubjectId.ForFolder("/srv/legacy/library"), new SubjectName("library"), new FailureReason("The Usher could not answer"));
        var findings = Sample.Reduce([lendingRun, folderRun]);
        findings = findings with { Id = _id };

        _statuses = [.. AnalysisStatuses.Build(_id, findings, located, [])];

        var target = new AnalysisTarget(AnalysisSubjectId.For(_lending), new SubjectName("Lending"), "/clones/core", "a41c9e2", RepositoryHost.GitHub, new RepositoryName("cratis/library-platform"), new BranchName("main"), new SubPath("services/core"));
        var running = new AnalysisProgress(_id, target, AnalysisState.Running, 3, [new SourcePath("Program.cs")], 10, null);
        var otherPrologue = running with { Prologue = PrologueId.New(), Target = target with { Subject = AnalysisSubjectId.ForFolder("/elsewhere") } };
        _whileRunning = [.. AnalysisStatuses.Build(_id, findings, located, [running, otherPrologue])];
    }

    [Fact] void should_list_the_located_system_with_its_last_analysis() => _statuses.First(_ => _.Name.Value == "Lending").State.ShouldEqual(AnalysisState.Complete);
    [Fact] void should_show_the_commit_and_files_of_the_last_analysis() => (_statuses[0].Commit!.Value, _statuses[0].TotalFiles.Value).ShouldEqual(("a41c9e2", 1412));
    [Fact] void should_show_where_the_code_lives() => (_statuses[0].Repository.Value, _statuses[0].Branch.Value, _statuses[0].SubPath.Value).ShouldEqual(("cratis/library-platform", "main", "services/core"));
    [Fact] void should_not_list_a_system_that_has_no_code_location() => _statuses.Exists(_ => _.Name.Value == "Search").ShouldBeFalse();
    [Fact] void should_list_an_analyzed_folder_after_the_systems() => _statuses[^1].Name.Value.ShouldEqual("library");
    [Fact] void should_show_why_a_folder_analysis_failed() => _statuses[^1].Reason!.Value.ShouldContain("Usher");
    [Fact] void should_show_a_failed_analysis_as_failed() => _statuses[^1].State.ShouldEqual(AnalysisState.Failed);
    [Fact] void should_prefer_what_is_running() => _whileRunning.First(_ => _.Name.Value == "Lending").State.ShouldEqual(AnalysisState.Running);
    [Fact] void should_show_how_many_files_are_read() => (_whileRunning[0].FilesRead.Value, _whileRunning[0].TotalFiles.Value).ShouldEqual((3, 10));
    [Fact] void should_ignore_the_progress_of_another_prologue() => _whileRunning.Count.ShouldEqual(2);
}
#endif
