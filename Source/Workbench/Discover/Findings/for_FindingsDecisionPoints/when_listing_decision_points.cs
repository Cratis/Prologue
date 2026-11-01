// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Discover.given;
using Cratis.Prologue.Workbench.Discover.Recording;

namespace Cratis.Prologue.Workbench.Discover.Findings.for_FindingsDecisionPoints;

public class when_listing_decision_points : Specification
{
    IReadOnlyList<DecisionPoint> _points;
    IReadOnlyList<DecisionPoint> _none;

    void Because()
    {
        var findings = Sample.Reduce(CodeAnalysisDecision.Decide(
            Sample.Outcome(new AnalysisFindings(
                [
                    Sample.Capability("Loans", "Lend copy", "POST /api/loans"),
                    Sample.Capability("Jobs", "Sweep overdue loans")
                ],
                [
                    Sample.Decision("Refuse a loan when no copy is available", 71),
                    Sample.Decision("Record the loan when a copy is available", 63, kind: DecisionKind.StateChange),
                    Sample.Decision("Sweep overdue loans every night", 12, "", DecisionKind.Schedule),
                    Sample.Decision("Refuse a renewal at the limit", 90, "POST /api/members/{id}/renew")
                ],
                [])),
            null));

        _points = FindingsDecisionPoints.From(findings);
        _none = FindingsDecisionPoints.From(NoFindings.For(findings.Id));
    }

    [Fact] void should_leave_out_a_schedule() => _points.Count.ShouldEqual(3);
    [Fact] void should_order_by_file_and_line() => _points.Select(point => point.Line.Value).ShouldContainOnly(63, 71, 90);
    [Fact] void should_turn_a_refusal_into_a_refused_outcome() => Find(71).Outcome.ShouldEqual(DecisionOutcome.Refused);
    [Fact] void should_turn_a_state_change_into_an_accepted_outcome() => Find(63).Outcome.ShouldEqual(DecisionOutcome.Accepted);
    [Fact] void should_carry_the_rule_file_and_shape() => (Find(71).Rule.Value, Find(71).File.Value, Find(71).Shape.Value).ShouldEqual(("Refuse a loan when no copy is available", "Loans/LoansController.cs", "POST /api/loans"));
    [Fact] void should_take_the_capability_whose_shapes_include_the_shape() => Find(71).Capability.Value.ShouldEqual("Lend copy");
    [Fact] void should_have_no_capability_when_no_capability_has_the_shape() => Find(90).Capability.ShouldEqual(Decisions.CapabilityName.None);
    [Fact] void should_list_nothing_for_findings_without_decision_points() => _none.ShouldBeEmpty();

    DecisionPoint Find(int line) => _points.First(point => point.Line.Value == line);
}
#endif
