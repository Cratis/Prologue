// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Live;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Decisions.for_BusinessDecision;

public class when_building_decisions : Specification
{
    IEnumerable<BusinessDecision> _decisions;

    void Because()
    {
        var context = EventContext.EmptyWithEventSourceId(PrologueId.New());
        RecentCaptures recent = null!;
        foreach (var @event in new[]
        {
            RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Update, "Inventory")),
            RecordedCaptures.Command("POST", "/api/loans", 409),
            RecordedCaptures.Command("POST", "/api/loans", 201, scenario: "Lend a copy")
        })
        {
            recent = RecentCaptures.Reduce(@event, recent, context);
        }

        var point = new DecisionPoint("POST /api/loans", DecisionOutcome.Refused, "a loan is refused when no copy is available", "LoansController.cs", 71, "Lending");
        _decisions = BusinessDecision.From(recent, [point]);
    }

    [Fact] void should_leave_out_captures_that_decided_nothing() => _decisions.Count().ShouldEqual(2);
    [Fact] void should_keep_the_newest_first() => _decisions.First().Outcome.ShouldEqual(DecisionOutcome.Accepted);
    [Fact] void should_carry_the_scenario() => _decisions.First().Scenario.Value.ShouldEqual("Lend a copy");
    [Fact] void should_explain_the_refusal_with_the_decision_point() => _decisions.Last().Reason.Text.Value.ShouldEqual("a loan is refused when no copy is available");
    [Fact] void should_explain_the_acceptance_with_the_observed_pattern() => _decisions.First().Reason.Source.ShouldEqual(ReasonSource.Observed);
}
#endif
