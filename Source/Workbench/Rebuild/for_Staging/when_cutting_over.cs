// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Rebuild.Differences;
using Cratis.Prologue.Workbench.Rebuild.given;
using Cratis.Prologue.Workbench.Rebuild.Staging;

namespace Cratis.Prologue.Workbench.Rebuild.for_Staging;

public class when_cutting_over : a_board
{
    Result<SliceCutOver, ValidationResult> _notShadowing;
    Result<SliceCutOver, ValidationResult> _neverCompared;
    Result<SliceCutOver, ValidationResult> _withDifferences;
    Result<SliceCutOver, ValidationResult> _afterAccepting;
    Result<SliceCutOver, ValidationResult> _afterMatching;
    Result<SliceCutOver, ValidationResult> _otherSliceDiffers;

    void Because()
    {
        MoveTo("LendBook", RebuildStage.Shadowing);
        MoveTo("ReturnBook", RebuildStage.Shadowing);
        _neverCompared = new CutOverSlice(_prologue, "LendBook").Handle(_board);

        Apply(Observation("POST /api/loans", ParityOutcome.Matched));
        var first = Observation("POST /api/loans", ParityOutcome.Mismatched, legacy: 201, shadow: 409);
        var second = Observation("POST /api/loans", ParityOutcome.Mismatched, legacy: 201, shadow: 409);
        Apply(first);
        Apply(second);
        Apply(Observation("POST /api/loans/{id}/return", ParityOutcome.Matched, path: "/api/loans/4412/return"));
        _withDifferences = new CutOverSlice(_prologue, "LendBook").Handle(_board);
        _otherSliceDiffers = new CutOverSlice(_prologue, "ReturnBook").Handle(_board);

        _board = RebuildBoard.Reduce(new DifferenceAccepted(first.Observation, first.Shape), _board, _context);
        _afterAccepting = new CutOverSlice(_prologue, "LendBook").Handle(_board);
        _board = RebuildBoard.Reduce(new DifferenceToBeMatched(second.Observation, second.Shape), _board, _context);
        _afterMatching = new CutOverSlice(_prologue, "LendBook").Handle(_board);
        _notShadowing = new CutOverSlice(_prologue, "NoSuchSlice").Handle(_board);
    }

    [Fact] void should_refuse_a_slice_that_was_never_compared() => _neverCompared.Refusal()!.Message.ShouldContain("not been compared");
    [Fact] void should_refuse_while_differences_are_unresolved() => _withDifferences.Refusal()!.Message.ShouldContain("2 unresolved differences");
    [Fact] void should_still_refuse_while_one_difference_is_unresolved() => _afterAccepting.Refusal()!.Message.ShouldContain("1 unresolved difference");
    [Fact] void should_cut_over_once_every_difference_is_resolved() => _afterMatching.Appended()!.Name.Value.ShouldEqual("LendBook");
    [Fact] void should_cut_over_a_slice_with_matching_decisions_only() => _otherSliceDiffers.Appended().ShouldNotBeNull();
    [Fact] void should_refuse_a_slice_that_does_not_exist() => _notShadowing.Refusal().ShouldNotBeNull();
}
#endif
