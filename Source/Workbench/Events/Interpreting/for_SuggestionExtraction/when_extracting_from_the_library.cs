// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_SuggestionExtraction;


public class when_extracting_from_the_library : a_library_interpretation
{
    SuggestedEvent Suggested(string module, string mechanical) => _suggestions.Single(suggestion => suggestion.Key == Key(module, mechanical));

    [Fact] void should_identify_a_suggestion_by_the_module_and_the_mechanical_name() => _suggestions.Select(suggestion => suggestion.Key.Value).ShouldContain("Authors/AuthorCreated");
    [Fact] void should_suggest_an_event_per_module_and_mechanical_name() => _suggestions.Select(suggestion => suggestion.Key).Distinct().Count().ShouldEqual(_suggestions.Count);
    [Fact] void should_not_suggest_events_for_excluded_traffic() => _suggestions.Any(suggestion => suggestion.Evidence.Shapes.Any(shape => shape.Shape.Value.Contains("simulation"))).ShouldBeFalse();
    [Fact] void should_keep_the_mechanical_name_as_the_refined_one_when_nothing_was_refined() => Suggested("Authors", "AuthorCreated").Refined.ShouldEqual(Suggested("Authors", "AuthorCreated").Mechanical);
    [Fact] void should_name_the_stream_after_the_entity() => Suggested("Authors", "AuthorCreated").Stream.Value.ShouldEqual("Author");
    [Fact] void should_count_the_captures_that_evidence_the_event() => Suggested("Authors", "AuthorCreated").Evidence.Captures.ShouldEqual(3);
    [Fact] void should_name_the_command_shape_that_led_to_the_event() => Suggested("Authors", "AuthorCreated").Evidence.Shapes.Single().Shape.Value.ShouldEqual("POST /api/authors/register");
    [Fact] void should_show_the_table_change_behind_the_event() => Suggested("Authors", "AuthorCreated").Evidence.Tables.Single().Table.ShouldEqual("Authors");
    [Fact] void should_show_the_columns_the_change_wrote() => Suggested("Authors", "AuthorCreated").Evidence.Tables.Single().Columns.ShouldContainOnly("AuthorId", "Name");
    [Fact] void should_carry_the_properties_with_the_types_of_the_schema() => Suggested("Authors", "AuthorCreated").Properties.Single(property => property.Name == "Name").MaxLength.ShouldEqual(200);
    [Fact] void should_carry_the_scenario_labels_of_the_shape() => Suggested("Authors", "AuthorCreated").Evidence.Scenarios.ShouldContainOnly("Register an author");
    [Fact] void should_trust_an_event_seen_often_more_than_one_seen_once() => Suggested("Authors", "AuthorCreated").Confidence.Value.ShouldBeGreaterThan(Suggested("Books", "BookCreated").Confidence.Value);
    [Fact] void should_attach_the_code_that_refuses_the_command() => Suggested("Loans", "LoanCreated").Evidence.Code.Single().File.Value.ShouldEqual("LoanPolicy.cs");
    [Fact] void should_put_the_line_of_the_code_in_the_evidence() => Suggested("Loans", "LoanCreated").Evidence.Code.Single().Line.Value.ShouldEqual(22);
    [Fact] void should_suggest_a_constraint_from_a_unique_index() => Suggested("Books", "BookCreated").Notes.Any(note => note.Kind == NoteKind.Constraint && note.Text.Contains("Isbn")).ShouldBeTrue();
    [Fact] void should_suggest_a_rule_from_the_refusals() => Suggested("Loans", "LoanCreated").Notes.Any(note => note.Kind == NoteKind.Rule && note.Text.Contains("409")).ShouldBeTrue();
    [Fact] void should_suggest_a_rule_from_the_code() => Suggested("Loans", "LoanCreated").Notes.Any(note => note.Kind == NoteKind.Rule && note.Text.Contains("Member has 5 open loans")).ShouldBeTrue();
    [Fact] void should_describe_the_structure_with_references_to_the_suggestions() => _model.Slices.SelectMany(slice => slice.Events).Distinct().ShouldContain(Key("Authors", "AuthorCreated"));
    [Fact] void should_keep_the_commands_in_the_structure() => _model.Slices.SelectMany(slice => slice.Commands).Select(command => command.Name.Value).ShouldContain("RegisterAuthor");
}

#endif
