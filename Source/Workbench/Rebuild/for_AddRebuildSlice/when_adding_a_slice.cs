// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Rebuild.given;
using Cratis.Prologue.Workbench.Rebuild.Slicing;

namespace Cratis.Prologue.Workbench.Rebuild.for_AddRebuildSlice;

public class when_adding_a_slice : a_board
{
    Result<RebuildSliceAdded, ValidationResult> _fresh;
    Result<RebuildSliceAdded, ValidationResult> _first;
    Result<RebuildSliceAdded, ValidationResult> _blank;
    Result<RebuildSliceAdded, ValidationResult> _taken;
    Result<RebuildSliceAdded, ValidationResult> _noShape;
    Result<RebuildSliceAdded, ValidationResult> _malformed;
    Result<RebuildSliceAdded, ValidationResult> _owned;

    void Because()
    {
        _fresh = new AddRebuildSlice(_prologue, "  AddBook ", ["post /api/catalog/books", "POST /api/catalog/books"]).Handle(_board);
        _first = new AddRebuildSlice(_prologue, "AddBook", ["POST /api/catalog/books"]).Handle(null);
        _blank = new AddRebuildSlice(_prologue, " ", ["POST /api/x"]).Handle(_board);
        _taken = new AddRebuildSlice(_prologue, "lendbook", ["POST /api/x"]).Handle(_board);
        _noShape = new AddRebuildSlice(_prologue, "AddBook", []).Handle(_board);
        _malformed = new AddRebuildSlice(_prologue, "AddBook", ["/api/catalog/books"]).Handle(_board);
        _owned = new AddRebuildSlice(_prologue, "LoanBook", ["POST /api/loans"]).Handle(_board);
    }

    [Fact] void should_add_the_slice_without_the_spaces_around_the_name() => _fresh.Appended()!.Name.Value.ShouldEqual("AddBook");
    [Fact] void should_normalize_the_method_and_keep_a_shape_once() => _fresh.Appended()!.Shapes.Select(shape => shape.Value).ShouldContainOnly(["POST /api/catalog/books"]);
    [Fact] void should_add_the_first_slice_of_a_prologue() => _first.Appended().ShouldNotBeNull();
    [Fact] void should_refuse_a_blank_name() => _blank.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_a_name_that_is_taken_whatever_its_case() => _taken.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_a_slice_that_replaces_nothing() => _noShape.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_a_shape_without_a_method() => _malformed.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_a_shape_another_slice_replaces() => _owned.Refusal()!.Message.ShouldContain("LendBook");
    [Fact] void should_start_a_slice_as_observed() => RebuildBoard.Reduce(_fresh.Appended()!, _board, _context).Slices.Last().Stage.ShouldEqual(RebuildStage.Observed);
}
#endif
