// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Uncaptured.for_CodeOnlySuggestions;

public class when_suggesting_from_the_code : an_uncaptured_library
{
    IReadOnlyList<SuggestedEvent> _suggestions2;
    SuggestedEvent _reservation;

    void Because()
    {
        _suggestions2 = [.. CodeOnlySuggestions.From(_uncaptured, new HashSet<SuggestionKey>(), new HashSet<string>()).Select(item => item.Suggestion)];
        _reservation = _suggestions2.Single(suggestion => suggestion.Key.Value == "Reservations/ReservationCreated");
    }

    [Fact] void should_suggest_what_no_capture_shows() => _suggestions2.Select(suggestion => suggestion.Key.Value).ShouldContainOnly("Reservations/ReservationCreated", "Lending/LoanRenewed", "Lending/OverdueSweepCompleted");
    [Fact] void should_not_suggest_a_capability_whose_command_was_captured() => _suggestions2.Any(suggestion => suggestion.Key.Value.Contains("LoanCreated")).ShouldBeFalse();
    [Fact] void should_not_suggest_a_capability_the_captures_show_before_the_matrix_does() =>
        CodeOnlySuggestions.From(_uncaptured with { CapturedShapes = [new CommandShape("post /api/reservations")] }, new HashSet<SuggestionKey>(), new HashSet<string>()).Any(item => item.Suggestion.Key.Value == "Reservations/ReservationCreated").ShouldBeFalse();
    [Fact] void should_not_suggest_an_event_for_a_query() => _suggestions2.Any(suggestion => suggestion.Evidence.Shapes.Any(shape => shape.Shape.Value.StartsWith("GET"))).ShouldBeFalse();
    [Fact] void should_mark_it_as_found_only_in_code() => _reservation.Origin.ShouldEqual(SuggestionOrigin.CodeOnly);
    [Fact] void should_name_it_mechanically_from_the_route() => _reservation.Mechanical.Value.ShouldEqual("ReservationCreated");
    [Fact] void should_keep_the_mechanical_name_as_the_refined_one() => _reservation.Refined.ShouldEqual(_reservation.Mechanical);
    [Fact] void should_put_it_in_the_module_of_the_capability() => _reservation.Module.Value.ShouldEqual("Reservations");
    [Fact] void should_cite_the_file_of_the_capability() => _reservation.Evidence.Code.Single().File.Value.ShouldEqual("Reservations/Reserve-a-book.cs");
    [Fact] void should_list_the_uncaptured_shape_with_no_captures() => _reservation.Evidence.Shapes.Single().Captures.ShouldEqual(0);
    [Fact] void should_say_it_was_found_only_in_code() => _reservation.Notes.Any(note => note.Text == "Found only in code; no capture backs this yet.").ShouldBeTrue();
    [Fact] void should_be_less_sure_than_an_event_with_captures() => _reservation.Confidence.Value.ShouldBeLessThan(_suggestions.Min(suggestion => suggestion.Confidence.Value));
    [Fact] void should_have_no_properties() => _reservation.Properties.ShouldBeEmpty();
}
#endif
