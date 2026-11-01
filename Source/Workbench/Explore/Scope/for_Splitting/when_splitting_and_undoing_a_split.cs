// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Explore.Scope.Splitting;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Scope.for_Splitting;

public class when_splitting_and_undoing_a_split : Specification
{
    static readonly CommandShape _stock = "PUT /api/inventory/{id}/stock";
    PrologueId _prologue;
    EventContext _context;
    InterpretationScope _split;
    InterpretationScope _undone;
    Result<IEnumerable<ShapeSplitByStatus>, ValidationResult> _first;
    Result<IEnumerable<ShapeSplitByStatus>, ValidationResult> _again;
    Result<IEnumerable<ShapeSplitByStatus>, ValidationResult> _blank;
    Result<IEnumerable<ShapeSplitUndone>, ValidationResult> _undoNeverSplit;
    Result<IEnumerable<ShapeSplitUndone>, ValidationResult> _undo;

    void Because()
    {
        _prologue = PrologueId.New();
        _context = EventContext.EmptyWithEventSourceId(_prologue);
        _first = new SplitShapeByStatus(_prologue, _stock).Handle(null);
        _split = InterpretationScope.Reduce(Events(_first).Single(), null, _context);
        _again = new SplitShapeByStatus(_prologue, _stock).Handle(_split);
        _blank = new SplitShapeByStatus(_prologue, CommandShape.None).Handle(null);
        _undoNeverSplit = new UndoShapeSplit(_prologue, _stock).Handle(null);
        _undo = new UndoShapeSplit(_prologue, _stock).Handle(_split);
        _undone = InterpretationScope.Reduce(Events(_undo).Single(), _split, _context);
    }

    static T[] Events<T>(Result<IEnumerable<T>, ValidationResult> result) =>
        result.TryGetResult(out var events) ? [.. events] : [];

    [Fact] void should_record_the_split() => Events(_first).Single().Shape.ShouldEqual(_stock);
    [Fact] void should_interpret_the_shape_per_status() => InterpretationScopes.SplitsByStatus(_split, _stock).ShouldBeTrue();
    [Fact] void should_record_nothing_when_it_is_already_split() => Events(_again).ShouldBeEmpty();
    [Fact] void should_refuse_a_blank_shape() => _blank.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_record_nothing_when_undoing_a_split_that_never_was() => Events(_undoNeverSplit).ShouldBeEmpty();
    [Fact] void should_record_undoing_the_split() => Events(_undo).Single().Shape.ShouldEqual(_stock);
    [Fact] void should_interpret_the_shape_as_a_whole_again() => InterpretationScopes.SplitsByStatus(_undone, _stock).ShouldBeFalse();
    [Fact] void should_not_split_anything_when_nothing_was_ever_split() => InterpretationScopes.SplitsByStatus(null, _stock).ShouldBeFalse();
    [Fact] void should_keep_excluding_independent_of_splitting() => InterpretationScopes.Includes(_split, _stock).ShouldBeTrue();
}
#endif
