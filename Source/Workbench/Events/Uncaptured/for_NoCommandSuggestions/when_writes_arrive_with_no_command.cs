// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Uncaptured.for_NoCommandSuggestions;

public class when_writes_arrive_with_no_command : an_uncaptured_library
{
    IReadOnlyList<(SuggestedEvent Suggestion, CodeJob? Job)> _suggestions2;
    IReadOnlyList<RaisedQuestion> _questions;
    SuggestedEvent _inventory;

    void Because()
    {
        (_suggestions2, _questions) = NoCommandSuggestions.From(_uncaptured, []);
        _inventory = _suggestions2.Single().Suggestion;
    }

    [Fact] void should_suggest_an_event_for_the_table_a_job_explains() => _inventory.Key.Value.ShouldEqual("Background/dbo.Inventory.InventoryUpdated");
    [Fact] void should_tie_it_to_the_job() => _suggestions2.Single().Job.ShouldEqual(_job);
    [Fact] void should_mark_it_as_written_with_no_command() => _inventory.Origin.ShouldEqual(SuggestionOrigin.NoCommand);
    [Fact] void should_keep_the_mechanical_name_from_the_table_and_the_operation() => _inventory.Mechanical.Value.ShouldEqual("InventoryUpdated");
    [Fact] void should_name_the_event_after_what_the_job_does() => _inventory.Refined.Value.ShouldEqual("InventoryReconciled");
    [Fact] void should_put_it_in_the_module_of_the_job() => _inventory.Module.Value.ShouldEqual("Stock");
    [Fact] void should_cite_the_job_in_the_code() => _inventory.Evidence.Code.Single().File.Value.ShouldEqual("Jobs/InventoryJob.cs");
    [Fact] void should_cite_the_line_of_the_job() => _inventory.Evidence.Code.Single().Line.Value.ShouldEqual(14);
    [Fact] void should_show_the_table_change() => _inventory.Evidence.Tables.Single().Table.ShouldEqual("Inventory");
    [Fact] void should_have_no_captured_command() => _inventory.Evidence.Shapes.ShouldBeEmpty();
    [Fact] void should_say_when_the_writes_happen() => _inventory.Notes.Single().Text.ShouldContain("mostly around 02:00");
    [Fact] void should_trust_it_more_than_code_alone() => _inventory.Confidence.Value.ShouldBeGreaterThan(SuggestionConfidence.ForCodeOnly(3).Value);
    [Fact] void should_ask_about_the_table_no_job_explains() => _questions.Single().Prompt.ShouldEqual("Reminders changes 4 times with no command, mostly around 03:00. What is that?");
    [Fact] void should_offer_the_three_answers() => _questions.Single().Choices.Select(choice => choice.Label).ShouldContainOnly("A scheduled job", "Staff correcting data", "Ignore");
    [Fact] void should_key_the_question_by_the_table() => _questions.Single().Key.Value.ShouldEqual("uncorrelated|dbo.reminders");
    [Fact] void should_not_ask_about_a_single_stray_write() => _questions.Any(question => question.Prompt.StartsWith("Holds")).ShouldBeFalse();
}
#endif
