// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Rebuild.Differences;
using Cratis.Prologue.Workbench.Rebuild.given;
using Cratis.Prologue.Workbench.Rebuild.Parity;

namespace Cratis.Prologue.Workbench.Rebuild.for_Differences;

public class when_resolving_a_difference : a_board
{
    ParityObserved _first;
    ParityObserved _second;
    Result<DifferenceAccepted, ValidationResult> _accepted;
    Result<DifferenceAccepted, ValidationResult> _acceptedAgain;
    Result<DifferenceToBeMatched, ValidationResult> _matchAfterAccepting;
    Result<DifferenceToBeMatched, ValidationResult> _matched;
    Result<DifferenceAccepted, ValidationResult> _unknown;
    RebuildDifferences _afterAccepting;
    RebuildDifferences _afterMatching;

    void Because()
    {
        _first = Observation("POST /api/loans", ParityOutcome.Mismatched, legacy: 201, shadow: 409);
        _second = Observation("POST /api/loans", ParityOutcome.Mismatched, legacy: 500, shadow: 201);
        Apply(Observation("POST /api/loans", ParityOutcome.Matched));
        Apply(_first);
        Apply(_second);

        _accepted = new AcceptDifference(_prologue, _first.Observation).Handle(_differences);
        _afterAccepting = RebuildDifferences.Reduce(_accepted.Appended()!, _differences, _context);
        _acceptedAgain = new AcceptDifference(_prologue, _first.Observation).Handle(_afterAccepting);
        _matchAfterAccepting = new MatchLegacyForDifference(_prologue, _first.Observation).Handle(_afterAccepting);
        _matched = new MatchLegacyForDifference(_prologue, _second.Observation).Handle(_afterAccepting);
        _afterMatching = RebuildDifferences.Reduce(_matched.Appended()!, _afterAccepting, _context);
        _unknown = new AcceptDifference(_prologue, Guid.NewGuid()).Handle(_differences);
    }

    [Fact] void should_only_keep_mismatches_as_differences() => _differences!.Differences.Count().ShouldEqual(2);
    [Fact] void should_show_what_the_legacy_system_decided() => _differences!.Differences.Last().LegacyDecision.ShouldEqual(DecisionOutcome.Accepted);
    [Fact] void should_show_what_the_new_system_decided() => _differences!.Differences.Last().ShadowDecision.ShouldEqual(DecisionOutcome.Refused);
    [Fact] void should_accept_an_open_difference() => _accepted.Appended()!.Shape.Value.ShouldEqual("POST /api/loans");
    [Fact] void should_mark_the_difference_accepted() => _afterAccepting.Differences.Single(difference => difference.Observation == _first.Observation).Resolution.ShouldEqual(DifferenceResolution.Accepted);
    [Fact] void should_refuse_accepting_a_resolved_difference() => _acceptedAgain.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_matching_a_resolved_difference() => _matchAfterAccepting.Refusal().ShouldNotBeNull();
    [Fact] void should_record_matching_the_legacy_behavior() => _afterMatching.Differences.Single(difference => difference.Observation == _second.Observation).Resolution.ShouldEqual(DifferenceResolution.ToBeMatched);
    [Fact] void should_refuse_a_difference_that_does_not_exist() => _unknown.Refusal().ShouldNotBeNull();
    [Fact] void should_leave_no_open_difference_on_the_slice() => RebuildBoard.Reduce(_matched.Appended()!, RebuildBoard.Reduce(_accepted.Appended()!, _board, _context), _context).Slices.First().OpenDifferences.ShouldBeEmpty();
}
#endif
