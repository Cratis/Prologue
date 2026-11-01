// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Discover.given;

namespace Cratis.Prologue.Workbench.Discover.Recording.for_CodeAnalysisDecision;

public class when_analyzing_the_same_code_and_the_model_words_it_differently : Specification
{
    List<object> _reworded;
    List<object> _kindChanged;
    List<object> _detailAdded;

    void Because()
    {
        var first = CodeAnalysisDecision.Decide(
            Sample.Outcome(new AnalysisFindings(
                [Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return"), Sample.Capability("Jobs", "Sweep overdue loans")],
                [Sample.Decision("Refuse a loan when no copy is available", 71)],
                [Sample.Dependency("Notifications", detail: "POST /notify"), Sample.Dependency("library-db", DependencyKind.Database, string.Empty)])),
            null);
        var recorded = Sample.Reduce(first);

        // The same lines of the same files, described in other words: the capability, the rule and the detail are reworded.
        _reworded = [.. CodeAnalysisDecision.Decide(
            Sample.Outcome(new AnalysisFindings(
                [Sample.Capability("Loans", "Give a loan back", "POST /api/loans/{id}/return"), Sample.Capability("Jobs", "Overdue loan sweep")],
                [Sample.Decision("No loan without a free copy", 71)],
                [Sample.Dependency("Notifications", detail: "sends a notice to the member"), Sample.Dependency("library-db", DependencyKind.Database, string.Empty)])),
            recorded)];

        // What the decision point is did change at the same line, so it is a new fact and the old one goes.
        _kindChanged = [.. CodeAnalysisDecision.Decide(
            Sample.Outcome(new AnalysisFindings(
                [Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return"), Sample.Capability("Jobs", "Sweep overdue loans")],
                [Sample.Decision("A loan needs a free copy", 71, kind: DecisionKind.StateChange)],
                [Sample.Dependency("Notifications", detail: "POST /notify"), Sample.Dependency("library-db", DependencyKind.Database, string.Empty)])),
            recorded)];

        // A dependency that had no detail gets one the first time the model gives it.
        _detailAdded = [.. CodeAnalysisDecision.Decide(
            Sample.Outcome(new AnalysisFindings(
                [Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return"), Sample.Capability("Jobs", "Sweep overdue loans")],
                [Sample.Decision("Refuse a loan when no copy is available", 71)],
                [Sample.Dependency("Notifications", detail: "POST /notify"), Sample.Dependency("library-db", DependencyKind.Database, "writes loans")])),
            recorded)];
    }

    [Fact] void should_record_nothing_when_only_the_wording_changed() => _reworded.OfType<object>().Where(@event => @event is not CodeAnalysisCompleted).ShouldBeEmpty();
    [Fact] void should_not_remove_and_find_the_capability_again() => _reworded.OfType<CapabilityRemoved>().ShouldBeEmpty();
    [Fact] void should_not_remove_and_find_the_decision_point_again() => (_reworded.OfType<DecisionPointRemoved>().Any() || _reworded.OfType<DecisionPointFound>().Any()).ShouldBeFalse();
    [Fact] void should_find_the_decision_point_whose_kind_changed() => _kindChanged.OfType<DecisionPointFound>().Single().Kind.ShouldEqual(DecisionKind.StateChange);
    [Fact] void should_remove_the_decision_point_it_replaces() => _kindChanged.OfType<DecisionPointRemoved>().Single().Rule.Value.ShouldContain("Refuse a loan");
    [Fact] void should_record_a_detail_that_was_missing() => _detailAdded.OfType<DependencyFound>().Single().Detail.Value.ShouldEqual("writes loans");
}
#endif
