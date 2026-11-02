// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Explore.Matrix;

namespace Cratis.Prologue.Workbench.Events.Uncaptured.for_NoCommandSuggestions;

public class when_two_tables_map_to_the_same_event : an_uncaptured_library
{
    IReadOnlyList<SuggestedEvent> _found;
    IReadOnlyList<SuggestedEvent> _again;

    protected override IEnumerable<CodeJob> Jobs() => [];

    protected override IEnumerable<Explore.Uncorrelated.UncorrelatedTable> Uncorrelated() =>
        [Written("Orders", 5, 2), Written("Order", 4, 2), Written("Orders", 3, 2, ChangeOperation.Update, "archive")];

    void Because()
    {
        var answers = new[] { "Orders", "Order" }.Select(table => new Questions.QuestionAnswer(UncorrelatedAnswers.KeyOf("dbo", table), "a prompt", UncorrelatedAnswers.ScheduledJob)).Concat([new Questions.QuestionAnswer(UncorrelatedAnswers.KeyOf("archive", "Orders"), "a prompt", UncorrelatedAnswers.ScheduledJob)]).ToList();
        _found = [.. NoCommandSuggestions.From(_uncaptured, answers).Suggestions.Select(item => item.Suggestion)];
        _again = [.. NoCommandSuggestions.From(_uncaptured with { Uncorrelated = [.. _uncaptured.Uncorrelated, .. _uncaptured.Uncorrelated] }, answers).Suggestions.Select(item => item.Suggestion)];
    }

    [Fact] void should_suggest_each_table() => _found.Count.ShouldEqual(3);
    [Fact] void should_give_each_suggestion_its_own_key() => _found.Select(suggestion => suggestion.Key).Distinct().Count().ShouldEqual(3);
    [Fact] void should_keep_the_key_stable() => _found.Select(suggestion => suggestion.Key.Value).ShouldContain("Background/archive.Orders.OrderUpdated");
    [Fact] void should_not_repeat_a_suggestion_when_a_table_is_listed_twice() => _again.Count.ShouldEqual(3);
    [Fact] void should_keep_the_evidence_of_a_table_listed_twice() => _again.Single(suggestion => suggestion.Key.Value == "Background/dbo.Order.OrderUpdated").Evidence.Tables.Count().ShouldEqual(1);
}
#endif
