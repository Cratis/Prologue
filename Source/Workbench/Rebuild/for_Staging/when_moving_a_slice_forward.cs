// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Rebuild.given;
using Cratis.Prologue.Workbench.Rebuild.Staging;

namespace Cratis.Prologue.Workbench.Rebuild.for_Staging;

public class when_moving_a_slice_forward : a_board
{
    Result<RebuildSliceMarkedBuilt, ValidationResult> _builtTooEarly;
    Result<RebuildSliceMarkedModeled, ValidationResult> _modeled;
    Result<RebuildSliceMarkedModeled, ValidationResult> _modeledTwice;
    Result<ShadowingStarted, ValidationResult> _shadowingTooEarly;
    Result<RebuildSliceMarkedModeled, ValidationResult> _unknown;

    void Because()
    {
        _builtTooEarly = new MarkRebuildSliceBuilt(_prologue, "LendBook").Handle(_board);
        _modeled = new MarkRebuildSliceModeled(_prologue, "LendBook").Handle(_board);
        MoveTo("LendBook", RebuildStage.Modeled);
        _modeledTwice = new MarkRebuildSliceModeled(_prologue, "LendBook").Handle(_board);
        _shadowingTooEarly = new StartShadowing(_prologue, "LendBook").Handle(_board);
        _unknown = new MarkRebuildSliceModeled(_prologue, "Nothing").Handle(_board);
    }

    [Fact] void should_refuse_building_a_slice_that_is_not_modeled() => _builtTooEarly.Refusal().ShouldNotBeNull();
    [Fact] void should_mark_an_observed_slice_modeled() => _modeled.Appended().ShouldNotBeNull();
    [Fact] void should_move_one_stage_at_a_time() => _modeledTwice.Refusal()!.Message.ShouldContain("Modeled");
    [Fact] void should_refuse_shadowing_a_slice_that_is_not_built() => _shadowingTooEarly.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_a_slice_that_does_not_exist() => _unknown.Refusal().ShouldNotBeNull();
    [Fact] void should_leave_the_other_slices_where_they_were() => Return.Stage.ShouldEqual(RebuildStage.Observed);
}
#endif
