// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discovery.Findings;
using Cratis.Prologue.Workbench.Discovery.given;

namespace Cratis.Prologue.Workbench.Discovery.Recording.for_CodeAnalysisDecision;

public class when_analyzing_again_and_the_code_changed : Specification
{
    List<object> _events;

    void Because()
    {
        var before = new AnalysisFindings(
            [Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return"), Sample.Capability("Loans", "Lend copy", "POST /api/loans")],
            [Sample.Decision("Refuse a loan when no copy is available"), Sample.Decision("Refuse a loan when the member has 5 open loans", 22)],
            [Sample.Dependency("Notifications"), Sample.Dependency("library-db", DependencyKind.Database, "writes loans")]);
        var first = CodeAnalysisDecision.Decide(Sample.Outcome(before), null);

        var after = new AnalysisFindings(
            [Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return", "POST /api/loans/{id}/return/late")],
            [Sample.Decision("Refuse a loan when no copy is available", 80)],
            [Sample.Dependency("Notifications", detail: "POST /notify/v2"), Sample.Dependency("library-db", DependencyKind.Database, "writes loans")]);
        _events = [.. CodeAnalysisDecision.Decide(Sample.Outcome(after, "b52dd10", 14), Sample.Reduce(first))];
    }

    [Fact] void should_record_the_capability_whose_shapes_changed() => _events.OfType<CapabilityMapped>().Single().Shapes.Count().ShouldEqual(2);
    [Fact] void should_record_the_capability_that_is_gone() => _events.OfType<CapabilityRemoved>().Single().Capability.Value.ShouldEqual("Lend copy");
    [Fact] void should_record_the_decision_point_that_moved() => _events.OfType<DecisionPointFound>().Single().Line.Value.ShouldEqual(80);
    [Fact] void should_record_the_decision_point_that_is_gone() => _events.OfType<DecisionPointRemoved>().Single().Rule.Value.ShouldContain("5 open loans");
    [Fact] void should_record_the_dependency_whose_detail_changed() => _events.OfType<DependencyFound>().Single().Detail.Value.ShouldEqual("POST /notify/v2");
    [Fact] void should_not_record_a_dependency_that_is_unchanged() => _events.OfType<DependencyRemoved>().ShouldBeEmpty();
    [Fact] void should_complete_at_the_new_commit() => _events.OfType<CodeAnalysisCompleted>().Single().Source.Value.ShouldEqual("b52dd10");
    [Fact] void should_record_exactly_the_changes() => _events.Count.ShouldEqual(6);
}
#endif
