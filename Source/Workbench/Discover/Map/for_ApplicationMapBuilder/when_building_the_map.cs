// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Discover.given;
using Cratis.Prologue.Workbench.Discover.Recording;
using Cratis.Prologue.Workbench.Discover.Runtime;

namespace Cratis.Prologue.Workbench.Discover.Map.for_ApplicationMapBuilder;

public class when_building_the_map : Specification
{
    ApplicationMap _map;
    IEnumerable<CodeDecisionPoint> _decisions;
    IEnumerable<CodeDependency> _dependencies;

    void Because()
    {
        var findings = Sample.Reduce(CodeAnalysisDecision.Decide(
            Sample.Outcome(new AnalysisFindings(
                [
                    Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return"),
                    Sample.Capability("Loans", "Lend copy", "POST /api/loans"),
                    Sample.Capability("Jobs", "Sweep overdue loans")
                ],
                [Sample.Decision("Refuse a loan when no copy is available"), Sample.Decision("Refuse a loan when the member has 5 open loans", 22, "POST /api/loans/{id}/renew")],
                [Sample.Dependency("Notifications")]),
                files: 12),
            null));
        var evidence = new RuntimeEvidence([new(new RouteShape("POST /api/loans"), [new(200, 902), new(409, 72)])], 0);

        _map = ApplicationMapBuilder.Build(findings, null, evidence);
        _decisions = CodeDecisionPoint.Build(findings, null, evidence);
        _dependencies = CodeDependency.Build(findings, null);
    }

    [Fact] void should_group_capabilities_by_system_and_module() => _map.Systems.Single().Modules.Select(_ => _.Name.Value).ShouldContainOnly("Jobs", "Loans");
    [Fact] void should_name_a_folder_from_its_analysis() => _map.Systems.Single().Name.Value.ShouldEqual("Lending");
    [Fact] void should_mark_a_capability_seen_when_its_command_shape_was_captured() => Find("Lend copy").Runtime.ShouldEqual(RuntimeStatus.Seen);
    [Fact] void should_carry_how_often_it_was_seen() => Find("Lend copy").Seen.ShouldEqual(974);
    [Fact] void should_mark_a_capability_found_only_in_code_when_its_command_was_never_captured() => Find("Return loan").Runtime.ShouldEqual(RuntimeStatus.CodeOnly);
    [Fact] void should_mark_a_capability_no_command_reaches_as_found_only_in_code() => Find("Sweep overdue loans").Runtime.ShouldEqual(RuntimeStatus.CodeOnly);
    [Fact] void should_count_the_capabilities() => (_map.Capabilities, _map.MatchedToRuntime, _map.CodeOnly).ShouldEqual((3, 1, 2));
    [Fact] void should_count_the_decision_points_and_files() => (_map.DecisionPoints, _map.FilesRead).ShouldEqual((2, 12));
    [Fact] void should_show_how_often_a_refusal_happened() => _decisions.First(_ => _.Line.Value == 71).Happened.ShouldEqual(72);
    [Fact] void should_flag_a_decision_point_never_exercised_as_code_only() => _decisions.First(_ => _.Line.Value == 22).Runtime.ShouldEqual(DecisionRuntime.CodeOnly);
    [Fact] void should_name_who_depends_on_what() => _dependencies.Single().From.Value.ShouldEqual("Lending");

    MapCapability Find(string name) => _map.Systems.SelectMany(_ => _.Modules).SelectMany(_ => _.Capabilities).First(_ => _.Capability.Value == name);
}
#endif
