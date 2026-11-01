// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Live;

namespace Cratis.Prologue.Workbench.Decisions.for_DecisionReasoning;

public class when_selecting : Specification
{
    static readonly DecisionPoint _noCopy = new("POST /api/loans", DecisionOutcome.Refused, "a loan is refused when no copy is available", "LoansController.cs", 71, "Lending");
    static readonly DecisionPoint _recorded = new("POST /api/loans", DecisionOutcome.Accepted, "a loan is recorded when a copy is available", "LoansController.cs", 63, "Lending");
    static readonly DecisionPoint _other = new("POST /api/loans/{id}/return", DecisionOutcome.Refused, "a loan can only be returned once", "LoansController.cs", 120, "Returns");

    DecisionReason _refusedWithPoint;
    DecisionReason _acceptedWithPoint;
    DecisionReason _refusedWithoutPoints;
    DecisionReason _refusedWithOnlyAnAcceptingPoint;
    DecisionReason _otherShape;

    static RecordedCapture Refused(string path = "/api/loans") => Capture(path, 409);

    static RecordedCapture Capture(string path, int status, RecordedTransaction[]? writes = null) =>
        RecordedCapture.From(RecordedCaptures.Command("POST", path, status, writes));

    void Because()
    {
        _refusedWithPoint = DecisionReasoning.Select(Refused(), [_recorded, _noCopy]);
        _acceptedWithPoint = DecisionReasoning.Select(Capture("/api/loans", 201), [_noCopy, _recorded]);
        _refusedWithoutPoints = DecisionReasoning.Select(Refused(), []);
        _refusedWithOnlyAnAcceptingPoint = DecisionReasoning.Select(Refused(), [_recorded]);
        _otherShape = DecisionReasoning.Select(Refused("/api/members"), [_noCopy, _other]);
    }

    [Fact] void should_use_the_rule_of_the_matching_decision_point() => _refusedWithPoint.Text.Value.ShouldEqual("a loan is refused when no copy is available");
    [Fact] void should_cite_its_file_and_line() => (_refusedWithPoint.File.Value, _refusedWithPoint.Line.Value).ShouldEqual(("LoansController.cs", 71));
    [Fact] void should_say_the_reason_was_found_in_the_code() => _refusedWithPoint.Source.ShouldEqual(ReasonSource.Discovered);
    [Fact] void should_carry_the_capability() => _refusedWithPoint.Capability.Value.ShouldEqual("Lending");
    [Fact] void should_match_an_acceptance_to_the_accepting_point() => _acceptedWithPoint.Text.Value.ShouldEqual("a loan is recorded when a copy is available");
    [Fact] void should_fall_back_to_the_observed_pattern() => _refusedWithoutPoints.Source.ShouldEqual(ReasonSource.Observed);
    [Fact] void should_describe_a_refusal_that_wrote_nothing() => _refusedWithoutPoints.Text.Value.ShouldEqual("nothing was written (409)");
    [Fact] void should_leave_the_capability_empty_without_points() => _refusedWithoutPoints.Capability.ShouldEqual(CapabilityName.None);
    [Fact] void should_not_explain_a_refusal_with_a_rule_that_accepts() => _refusedWithOnlyAnAcceptingPoint.Source.ShouldEqual(ReasonSource.Observed);
    [Fact] void should_still_know_the_capability_of_the_shape() => _refusedWithOnlyAnAcceptingPoint.Capability.Value.ShouldEqual("Lending");
    [Fact] void should_ignore_points_of_other_shapes() => _otherShape.Source.ShouldEqual(ReasonSource.Observed);
}
#endif
