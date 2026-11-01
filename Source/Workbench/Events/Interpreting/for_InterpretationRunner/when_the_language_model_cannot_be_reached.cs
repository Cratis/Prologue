// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Interpreting.given;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationRunner;

public class when_the_language_model_cannot_be_reached : an_interpretation_runner
{
    async Task Because()
    {
        _chatClients.Unreachable = "The Interpreter agent has no provider";
        await Run();
    }

    [Fact] void should_complete_with_the_heuristics() => Progress.State.ShouldEqual(InterpretationState.Completed);
    [Fact] void should_say_that_nothing_was_refined_and_why() => Progress.Message.ShouldContain("The Interpreter agent could not be used: The Interpreter agent has no provider");
    [Fact] void should_not_claim_the_result_was_refined() => Progress.Refined.ShouldBeFalse();
    [Fact] void should_record_the_interpretation_once() => _recorded.Count.ShouldEqual(1);
    [Fact] void should_record_the_suggestions() => _recorded[0].Suggestions.Select(suggestion => suggestion.Key.Value).ShouldContain("Authors/AuthorCreated");
    [Fact] void should_leave_the_mechanical_names_as_they_are() => _recorded[0].Suggestions.All(suggestion => suggestion.Refined == suggestion.Mechanical).ShouldBeTrue();
    [Fact] void should_record_the_boundaries() => _recorded[0].Boundaries.Any().ShouldBeTrue();
    [Fact] void should_record_what_is_not_an_event() => _recorded[0].NotEvents.Any().ShouldBeTrue();
    [Fact] void should_leave_the_excluded_traffic_out() => _recorded[0].Suggestions.Any(suggestion => suggestion.Evidence.Shapes.Any(shape => shape.Shape.Value.Contains("simulation"))).ShouldBeFalse();
    [Fact] void should_report_how_many_events_it_suggested() => Progress.Suggestions.ShouldEqual(_recorded[0].Suggestions.Count());
}
#endif
