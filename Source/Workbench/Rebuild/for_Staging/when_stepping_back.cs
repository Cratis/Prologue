// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Rebuild.given;
using Cratis.Prologue.Workbench.Rebuild.Slicing;
using Cratis.Prologue.Workbench.Rebuild.Staging;

namespace Cratis.Prologue.Workbench.Rebuild.for_Staging;

public class when_stepping_back : a_board
{
    Result<RebuildSliceSteppedBack, ValidationResult> _observed;
    Result<RebuildSliceSteppedBack, ValidationResult> _cutOver;
    Result<RebuildSliceRemoved, ValidationResult> _removeCutOver;
    Result<RebuildSliceRemoved, ValidationResult> _removeObserved;

    void Because()
    {
        _observed = new StepRebuildSliceBack(_prologue, "ReturnBook").Handle(_board);
        MoveTo("LendBook", RebuildStage.CutOver);
        _cutOver = new StepRebuildSliceBack(_prologue, "LendBook").Handle(_board);
        _removeCutOver = new RemoveRebuildSlice(_prologue, "LendBook").Handle(_board);
        _removeObserved = new RemoveRebuildSlice(_prologue, "ReturnBook").Handle(_board);
    }

    [Fact] void should_refuse_stepping_back_from_observed() => _observed.Refusal().ShouldNotBeNull();
    [Fact] void should_step_a_cut_over_slice_back_to_shadowing() => _cutOver.Appended()!.To.ShouldEqual(RebuildStage.Shadowing);
    [Fact] void should_apply_the_step_back() => RebuildBoard.Reduce(_cutOver.Appended()!, _board, _context).Slices.First().Stage.ShouldEqual(RebuildStage.Shadowing);
    [Fact] void should_refuse_removing_a_slice_that_serves_its_route() => _removeCutOver.Refusal().ShouldNotBeNull();
    [Fact] void should_remove_a_slice_that_is_not_serving() => RebuildBoard.Reduce(_removeObserved.Appended()!, _board, _context).Slices.Select(slice => slice.Name.Value).ShouldContainOnly(["LendBook"]);
}
#endif
