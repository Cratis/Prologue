// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Discover.given;

namespace Cratis.Prologue.Workbench.Discover.Recording.for_CodeAnalysisDecision;

public class when_analyzing_for_the_first_time : Specification
{
    List<object> _events;

    void Because() => _events = [.. CodeAnalysisDecision.Decide(
        Sample.Outcome(new AnalysisFindings(
            [Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return")],
            [Sample.Decision("Refuse a loan when no copy is available")],
            [Sample.Dependency("Notifications")])),
        null)];

    [Fact] void should_map_the_capability() => _events.OfType<CapabilityMapped>().Single().Capability.Value.ShouldEqual("Return loan");
    [Fact] void should_find_the_decision_point() => _events.OfType<DecisionPointFound>().Single().Line.Value.ShouldEqual(71);
    [Fact] void should_find_the_dependency() => _events.OfType<DependencyFound>().Single().Target.Value.ShouldEqual("Notifications");
    [Fact] void should_complete_the_analysis() => _events.OfType<CodeAnalysisCompleted>().Single().Source.Value.ShouldEqual("a41c9e2");
    [Fact] void should_complete_last() => _events[^1].ShouldBeOfExactType<CodeAnalysisCompleted>();
}
#endif
