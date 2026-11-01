// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Interpreting.given;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationRunner;

public class when_the_language_model_refines : an_interpretation_runner
{
    async Task Because()
    {
        _responses.Enqueue(Refinement);
        await Run();
    }

    [Fact] void should_complete() => Progress.State.ShouldEqual(InterpretationState.Completed);
    [Fact] void should_say_it_was_refined() => Progress.Message.ShouldContain("Refined by the Interpreter agent");
    [Fact] void should_show_the_refined_name() => _recorded[0].Suggestions.Single(suggestion => suggestion.Key.Value == "Authors/AuthorCreated").Refined.Value.ShouldEqual("AuthorRegistered");
    [Fact] void should_keep_the_key_of_the_mechanical_name() => _recorded[0].Suggestions.Single(suggestion => suggestion.Refined.Value == "AuthorRegistered").Key.Value.ShouldEqual("Authors/AuthorCreated");
    [Fact] void should_keep_the_system_name() => _recorded[0].Model.SystemName.ShouldEqual("Library");
    [Fact] void should_give_the_language_model_the_scenario_labels() => _sent[0].ShouldContain("Register an author");
    [Fact] void should_give_the_language_model_the_rules_found_in_the_code() => _sent[0].ShouldContain("Member has 5 open loans");
    [Fact] void should_not_give_the_language_model_the_excluded_traffic() => _sent[0].ShouldNotContain("simulation");
}
#endif
