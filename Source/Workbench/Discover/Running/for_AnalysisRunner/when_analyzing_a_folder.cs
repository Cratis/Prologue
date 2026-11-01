// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover.Analyzing;
using Cratis.Prologue.Workbench.Discover.given;
using Cratis.Prologue.Workbench.Discover.Recording;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discover.Running.for_AnalysisRunner;

public class when_analyzing_a_folder : a_source_folder
{
    AnalysisProgressStore _progress;
    FakeAgents _agents;
    AnalysisOutcome _outcome;
    AnalysisOutcome _withoutAgents;
    AnalysisOutcome _emptyFolder;

    AnalysisJob Job(string directory) => new(
        PrologueId.New(),
        new AnalysisTarget(AnalysisSubjectId.ForFolder(directory), new SubjectName("library"), directory, directory, RepositoryHost.LocalFolder, new RepositoryName(directory), new BranchName(string.Empty), new SubPath(string.Empty)));

    async Task Because()
    {
        _progress = new AnalysisProgressStore();
        Directory.CreateDirectory(Path.Combine(_root, "empty"));
        _agents = new FakeAgents(new AnalysisFindings(
            [Sample.Capability("Jobs", "Sweep overdue loans")],
            [Sample.Decision("Refuse a loan when no copy is available", 6, "POST /api/loans")],
            [Sample.Dependency("Notifications")]));

        _outcome = await new AnalysisRunner(_agents, _progress).Run(Job(_root), CancellationToken.None);
        _withoutAgents = await new AnalysisRunner(new FakeAgents(AnalysisFindings.Empty, "The Dramaturg could not answer: no provider", "The Prompter could not answer: no provider", "The Usher could not answer: no provider"), _progress).Run(Job(_root), CancellationToken.None);
        _emptyFolder = await new AnalysisRunner(new FakeAgents(AnalysisFindings.Empty), _progress).Run(Job(Path.Combine(_root, "empty")), CancellationToken.None);
    }

    [Fact] void should_map_the_controller_without_any_agent() => _withoutAgents.Findings.Capabilities.Select(_ => _.Capability.Value).ShouldContainOnly("Lend copy", "Return loan");
    [Fact] void should_map_the_capabilities_of_the_code_and_the_agents() => _outcome.Findings.Capabilities.Select(_ => _.Capability.Value).ShouldContainOnly("Lend copy", "Return loan", "Sweep overdue loans");
    [Fact] void should_find_the_decision_points_with_file_and_line() => (_outcome.Findings.DecisionPoints.Single().File.Value, _outcome.Findings.DecisionPoints.Single().Line.Value).ShouldEqual(("Loans/LoansController.cs", 6));
    [Fact] void should_find_the_dependencies() => _outcome.Findings.Dependencies.Single().Target.Value.ShouldEqual("Notifications");
    [Fact] void should_finish_without_a_failure() => _outcome.Failure.ShouldBeNull();
    [Fact] void should_count_the_files_read() => _outcome.Files.Value.ShouldEqual(3);
    [Fact] void should_show_the_agents_only_the_source_files() => _agents.Batches.SelectMany(_ => _.Files).Select(_ => _.Path.Value).ShouldContainOnly("Loans/LoanPolicy.cs", "Loans/LoansController.cs", "Program.cs");
    [Fact] void should_say_when_the_agents_could_not_read_everything() => _withoutAgents.Failure.Value.ShouldContain("did not read everything");
    [Fact] void should_say_which_agent_was_the_problem() => _withoutAgents.Failure.Value.ShouldContain("Prompter");
    [Fact] void should_fail_rather_than_complete_when_there_is_nothing_to_read() => _emptyFolder.Failure.Value.ShouldContain("No source files");
    [Fact] void should_report_how_many_files_there_are() => _progress.Get(AnalysisSubjectId.ForFolder(_root))!.TotalFiles.ShouldEqual(3);
}
#endif
