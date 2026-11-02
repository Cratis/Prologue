// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Uncaptured.for_UncapturedEvents;

public class when_extending_an_interpretation : an_uncaptured_library
{
    IReadOnlyList<SuggestedEvent> _all;
    InterpretedModel _extended;
    IReadOnlyList<RaisedQuestion> _questions;

    void Because() => (_all, _extended, _questions) = UncapturedEvents.Extend(_suggestions, _model, _uncaptured, []);

    ModelSlice SliceOf(string key) => _extended.Slices.Single(slice => slice.Events.Contains((SuggestionKey)key));

    [Fact] void should_keep_what_the_captures_gave() => _all.Take(_suggestions.Count).ShouldContainOnly(_suggestions);
    [Fact] void should_add_the_code_only_events() => _all.Count(suggestion => suggestion.Origin == SuggestionOrigin.CodeOnly).ShouldEqual(3);
    [Fact] void should_add_the_event_of_the_job() => _all.Count(suggestion => suggestion.Origin == SuggestionOrigin.NoCommand).ShouldEqual(1);
    [Fact] void should_not_suggest_a_job_twice_as_code_only_and_as_writes() => _all.Select(suggestion => suggestion.Key).Distinct().Count().ShouldEqual(_all.Count);
    [Fact] void should_raise_the_question_nothing_answers() => _questions.Count.ShouldEqual(1);
    [Fact] void should_describe_a_code_only_command_as_a_state_change_slice() => SliceOf("Reservations/ReservationCreated").Type.ShouldEqual(ExtractedSliceType.StateChange);
    [Fact] void should_give_the_slice_a_command_named_after_the_capability() => SliceOf("Reservations/ReservationCreated").Commands.Single().Name.Value.ShouldEqual("ReserveABook");
    [Fact] void should_describe_a_capability_without_a_route_as_an_automation() => SliceOf("Lending/OverdueSweepCompleted").Type.ShouldEqual(ExtractedSliceType.Automation);
    [Fact] void should_describe_the_writes_of_a_job_as_an_automation() => SliceOf("Background/dbo.Inventory.InventoryUpdated").Type.ShouldEqual(ExtractedSliceType.Automation);
    [Fact] void should_keep_the_slices_of_the_captures() => _extended.Slices.Count().ShouldEqual(_model.Slices.Count() + 4);
    [Fact] void should_not_change_the_structure_without_uncaptured_evidence() => UncapturedEvents.Extend(_suggestions, _model, _facts, []).Model.ShouldEqual(_model);
}
#endif
