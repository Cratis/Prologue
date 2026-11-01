// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Explore.Scope.Excluding;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Scope.for_Excluding;

public class when_excluding_and_including_a_shape : Specification
{
    static readonly CommandShape _load = "POST /api/simulation/{id}";
    PrologueId _prologue;
    EventContext _context;
    InterpretationScope _excluded;
    InterpretationScope _included;
    Result<IEnumerable<ShapeExcludedFromInterpretation>, ValidationResult> _first;
    Result<IEnumerable<ShapeExcludedFromInterpretation>, ValidationResult> _again;
    Result<IEnumerable<ShapeExcludedFromInterpretation>, ValidationResult> _blank;
    Result<IEnumerable<ShapeIncludedInInterpretation>, ValidationResult> _includeNeverExcluded;
    Result<IEnumerable<ShapeIncludedInInterpretation>, ValidationResult> _include;

    void Because()
    {
        _prologue = PrologueId.New();
        _context = EventContext.EmptyWithEventSourceId(_prologue);
        _first = new ExcludeShapeFromInterpretation(_prologue, _load).Handle(null);
        _excluded = InterpretationScope.Reduce(Events(_first).Single(), null, _context);
        _again = new ExcludeShapeFromInterpretation(_prologue, _load).Handle(_excluded);
        _blank = new ExcludeShapeFromInterpretation(_prologue, CommandShape.None).Handle(null);
        _includeNeverExcluded = new IncludeShapeInInterpretation(_prologue, _load).Handle(null);
        _include = new IncludeShapeInInterpretation(_prologue, _load).Handle(_excluded);
        _included = InterpretationScope.Reduce(Events(_include).Single(), _excluded, _context);
    }

    static T[] Events<T>(Result<IEnumerable<T>, ValidationResult> result) =>
        result.TryGetResult(out var events) ? [.. events] : [];

    [Fact] void should_record_the_exclusion() => Events(_first).Single().Shape.ShouldEqual(_load);
    [Fact] void should_leave_the_shape_out_of_the_scope() => InterpretationScopes.Includes(_excluded, _load).ShouldBeFalse();
    [Fact] void should_record_nothing_when_it_is_already_excluded() => Events(_again).ShouldBeEmpty();
    [Fact] void should_not_refuse_excluding_it_again() => _again.TryGetError(out _).ShouldBeFalse();
    [Fact] void should_refuse_a_blank_shape() => _blank.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_record_nothing_when_including_what_was_never_excluded() => Events(_includeNeverExcluded).ShouldBeEmpty();
    [Fact] void should_record_the_inclusion() => Events(_include).Single().Shape.ShouldEqual(_load);
    [Fact] void should_bring_the_shape_back_into_the_scope() => InterpretationScopes.Includes(_included, _load).ShouldBeTrue();
    [Fact] void should_include_everything_when_nothing_was_ever_excluded() => InterpretationScopes.Includes(null, _load).ShouldBeTrue();
    [Fact] void should_leave_other_shapes_in_the_scope() => InterpretationScopes.Includes(_excluded, "POST /api/loans").ShouldBeTrue();
    [Fact] void should_not_list_a_shape_twice_when_events_replay() => InterpretationScope.Reduce(new ShapeExcludedFromInterpretation(_load), _excluded, _context).Excluded.Count().ShouldEqual(1);
}
#endif
