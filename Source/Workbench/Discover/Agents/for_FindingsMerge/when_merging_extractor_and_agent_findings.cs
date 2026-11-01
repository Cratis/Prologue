// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover.given;

namespace Cratis.Prologue.Workbench.Discover.Agents.for_FindingsMerge;

public class when_merging_extractor_and_agent_findings : Specification
{
    AnalysisFindings _merged;

    void Because()
    {
        var extractor = new AnalysisFindings([Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return")], [], []);
        var agents = new AnalysisFindings(
            [
                Sample.Capability("Lending", "Return a copy", "post /api/loans/{id}/return", "POST /api/loans/{id}/return/late"),
                Sample.Capability("Jobs", "Sweep overdue loans"),
                Sample.Capability("Jobs", "sweep overdue loans!")
            ],
            [Sample.Decision("Refuse a loan when no copy is available"), Sample.Decision("refuse a loan when no copy is available.")],
            [Sample.Dependency("Notifications"), Sample.Dependency("notifications")]);
        _merged = FindingsMerge.Merge([extractor, agents]);
    }

    [Fact] void should_fold_an_agent_capability_that_shares_a_shape_into_the_extractors() => _merged.Capabilities.Count(_ => _.Module.Value == "Loans").ShouldEqual(1);
    [Fact] void should_keep_the_extractors_names() => _merged.Capabilities.Select(_ => _.Capability.Value).ShouldContain("Return loan");
    [Fact] void should_not_keep_the_agents_name_for_a_folded_capability() => _merged.Capabilities.Select(_ => _.Capability.Value).ShouldNotContain("Return a copy");
    [Fact] void should_add_the_extra_shape_the_agent_found() => _merged.Capabilities.First(_ => _.Module.Value == "Loans").Shapes.Count().ShouldEqual(2);
    [Fact] void should_add_what_only_the_agents_found() => _merged.Capabilities.Count(_ => _.Module.Value == "Jobs").ShouldEqual(1);
    [Fact] void should_drop_repeated_decision_points() => _merged.DecisionPoints.Count().ShouldEqual(1);
    [Fact] void should_drop_repeated_dependencies() => _merged.Dependencies.Count().ShouldEqual(1);
}
#endif
