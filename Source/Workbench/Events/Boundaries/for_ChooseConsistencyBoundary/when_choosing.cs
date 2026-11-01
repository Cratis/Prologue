// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Boundaries.for_ChooseConsistencyBoundary;

public class when_choosing : a_library_interpretation
{
    static readonly BoundaryKey _key = "POST /api/loans/{id}|across";
    EventReview _withBoundaries;
    Result<IEnumerable<ConsistencyBoundaryChosen>, ValidationResult> _chosen;
    Result<IEnumerable<ConsistencyBoundaryChosen>, ValidationResult> _again;
    Result<IEnumerable<ConsistencyBoundaryChosen>, ValidationResult> _changed;
    Result<IEnumerable<ConsistencyBoundaryChosen>, ValidationResult> _noSuchOption;
    Result<IEnumerable<ConsistencyBoundaryChosen>, ValidationResult> _noSuchBoundary;
    Result<IEnumerable<ConsistencyBoundaryChosen>, ValidationResult> _nothingDerived;
    EventReview _decided;

    void Because()
    {
        _withBoundaries = EventReview.Reduce(new InterpretationFindingsRecorded(BoundaryDetection.Detect(_facts, _scope), []), _review, _context);
        _chosen = new ChooseConsistencyBoundary(_prologue, _key, BoundaryDetection.OneStream).Handle(_withBoundaries);
        _decided = EventReview.Reduce(_chosen.TryGetResult(out var events) ? events.Single() : null!, _withBoundaries, _context);
        _again = new ChooseConsistencyBoundary(_prologue, _key, BoundaryDetection.OneStream).Handle(_decided);
        _changed = new ChooseConsistencyBoundary(_prologue, _key, BoundaryDetection.Dynamic).Handle(_decided);
        _noSuchOption = new ChooseConsistencyBoundary(_prologue, _key, "wishful-thinking").Handle(_withBoundaries);
        _noSuchBoundary = new ChooseConsistencyBoundary(_prologue, "POST /nowhere|across", BoundaryDetection.Dynamic).Handle(_withBoundaries);
        _nothingDerived = new ChooseConsistencyBoundary(_prologue, _key, BoundaryDetection.Dynamic).Handle(null);
    }

    static ConsistencyBoundaryChosen[] Events(Result<IEnumerable<ConsistencyBoundaryChosen>, ValidationResult> result) => result.TryGetResult(out var events) ? [.. events] : [];

    [Fact] void should_record_the_choice() => Events(_chosen).Single().Option.ShouldEqual(BoundaryDetection.OneStream);
    [Fact] void should_record_the_rule_the_choice_adds() => Events(_chosen).Single().Rule.ShouldContain("consistency boundary");
    [Fact] void should_keep_the_choice_in_the_review() => _decided.Choices.Single().Option.ShouldEqual(BoundaryDetection.OneStream);
    [Fact] void should_record_nothing_when_the_choice_is_the_one_made() => Events(_again).ShouldBeEmpty();
    [Fact] void should_record_a_changed_mind() => Events(_changed).Single().Option.ShouldEqual(BoundaryDetection.Dynamic);
    [Fact] void should_refuse_an_option_the_boundary_does_not_have() => _noSuchOption.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_refuse_a_boundary_the_evidence_does_not_raise() => _noSuchBoundary.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_refuse_when_nothing_was_derived() => _nothingDerived.TryGetError(out _).ShouldBeTrue();
}
#endif
