// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;

namespace Cratis.Prologue.Workbench.Events.Boundaries.for_BoundaryDetection;

public class when_detecting_boundaries : a_library_interpretation
{
    IReadOnlyList<ConsistencyBoundary> _boundaries;
    IReadOnlyList<ConsistencyBoundary> _again;
    IReadOnlyList<ConsistencyBoundary> _withoutLoans;

    void Because()
    {
        _boundaries = BoundaryDetection.Detect(_facts, _scope);
        _again = BoundaryDetection.Detect(_facts, _scope);
        _withoutLoans = BoundaryDetection.Detect(_facts with { Shapes = [.. _facts.Shapes.Where(shape => !shape.Shape.Value.Contains("loans"))] }, _scope);
    }

    ConsistencyBoundary Across => _boundaries.Single(boundary => boundary.Key.Value == "POST /api/loans/{id}|across");
    ConsistencyBoundary Removal => _boundaries.Single(boundary => boundary.Key.Value == "DELETE /api/authors/{id}|removal");

    [Fact] void should_raise_a_boundary_for_a_command_that_writes_two_streams_and_is_refused() => Across.Shape.Value.ShouldEqual("POST /api/loans/{id}");
    [Fact] void should_use_the_rule_the_code_holds() => Across.Rule.ShouldEqual("Member has 5 open loans");
    [Fact] void should_say_which_tables_are_written_together() => Across.Evidence.ShouldContain("Loans, Inventory");
    [Fact] void should_say_how_often_it_is_refused() => Across.Evidence.ShouldContain("Refused 2 times with 409");
    [Fact] void should_suggest_deciding_on_one_stream_first() => Across.Options.First().Key.ShouldEqual(BoundaryDetection.OneStream);
    [Fact] void should_offer_a_dynamic_consistency_boundary() => Across.Options.Select(option => option.Key).ShouldContain(BoundaryDetection.Dynamic);
    [Fact] void should_give_every_option_the_rule_it_adds_to_the_model() => Across.Options.All(option => option.Rule.Length > 0).ShouldBeTrue();
    [Fact] void should_raise_a_boundary_for_a_removal_that_is_refused_while_rows_refer_to_it() => Removal.Evidence.ShouldContain("Books");
    [Fact] void should_describe_the_removal_rule() => Removal.Rule.ShouldContain("cannot be removed");
    [Fact] void should_offer_to_allow_the_removal() => Removal.Options.Select(option => option.Key).ShouldContain(BoundaryDetection.KeepReferences);
    [Fact] void should_not_raise_a_boundary_for_a_command_that_is_never_refused() => _boundaries.Any(boundary => boundary.Shape.Value == "POST /api/authors/register").ShouldBeFalse();
    [Fact] void should_not_raise_a_boundary_for_a_command_that_writes_one_stream() => _boundaries.Any(boundary => boundary.Shape.Value == "POST /api/books/register").ShouldBeFalse();
    [Fact] void should_derive_the_same_keys_every_time() => _again.Select(boundary => boundary.Key).ShouldEqual(_boundaries.Select(boundary => boundary.Key));
    [Fact] void should_not_raise_a_boundary_for_a_shape_that_was_not_captured() => _withoutLoans.Any(boundary => boundary.Shape.Value.Contains("loans")).ShouldBeFalse();
}
#endif
