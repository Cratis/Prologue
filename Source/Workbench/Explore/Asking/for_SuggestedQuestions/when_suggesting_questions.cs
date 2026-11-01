// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Explore.given;

namespace Cratis.Prologue.Workbench.Explore.Asking.for_SuggestedQuestions;

public class when_suggesting_questions : a_lending_library
{
    SuggestedQuestion[] _suggestions;
    SuggestedQuestion[] _none;

    void Because()
    {
        _suggestions = [.. SuggestedQuestions.For(_matrix, _uncorrelated, _scope)];
        _none = [.. SuggestedQuestions.For(null, null, null)];
    }

    SuggestedQuestion Asked(string start) => _suggestions.Single(suggestion => suggestion.Question.StartsWith(start, StringComparison.Ordinal));

    [Fact] void should_ask_what_the_most_captured_command_does() => Asked("What happens when POST /api/loans runs").Answer.ShouldContain("captured 5 times and answered 201 in 80% and 409 in 20%");
    [Fact] void should_answer_with_the_writes_and_shares_from_the_matrix() => Asked("What happens when POST /api/loans runs").Answer.ShouldContain("Loans (insert 80%)");
    [Fact] void should_cite_the_shape_the_answer_is_about() => Asked("What happens when POST /api/loans runs").Citations.Single().Value.ShouldEqual("POST /api/loans");
    [Fact] void should_ask_which_commands_change_the_table_the_most_commands_change() => Asked("Which commands change Inventory").Citations.Select(shape => shape.Value).ShouldContainOnly(["POST /api/loans", "PUT /api/inventory/{id}/stock"]);
    [Fact] void should_ask_why_commands_are_refused() => Asked("Why are some commands refused").Answer.ShouldContain("20% of runs answer 409 and write nothing");
    [Fact] void should_ask_about_the_writes_outside_any_command() => Asked("Which writes happen outside any command").Answer.ShouldContain("Inventory in 1 transactions around 02:00 UTC");
    [Fact] void should_cite_the_commands_that_write_the_same_tables_as_those_outside_any_command() => Asked("Which writes happen outside any command").Citations.Count().ShouldEqual(2);
    [Fact] void should_not_ask_about_shapes_excluded_from_interpretation() => _suggestions.Any(suggestion => suggestion.Question.Contains("simulation", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_suggest_nothing_when_nothing_was_captured() => _none.ShouldBeEmpty();
}
#endif
