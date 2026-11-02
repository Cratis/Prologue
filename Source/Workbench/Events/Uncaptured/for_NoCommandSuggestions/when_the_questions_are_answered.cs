// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;

namespace Cratis.Prologue.Workbench.Events.Uncaptured.for_NoCommandSuggestions;

public class when_the_questions_are_answered : an_uncaptured_library
{
    static QuestionAnswer Answer(string table, string answer) => new(UncorrelatedAnswers.KeyOf("dbo", table), "a prompt", answer);

    (IReadOnlyList<(SuggestedEvent Suggestion, CodeJob? Job)> Suggestions, IReadOnlyList<RaisedQuestion> Questions) _job;
    (IReadOnlyList<(SuggestedEvent Suggestion, CodeJob? Job)> Suggestions, IReadOnlyList<RaisedQuestion> Questions) _correction;
    (IReadOnlyList<(SuggestedEvent Suggestion, CodeJob? Job)> Suggestions, IReadOnlyList<RaisedQuestion> Questions) _ignored;
    (IReadOnlyList<(SuggestedEvent Suggestion, CodeJob? Job)> Suggestions, IReadOnlyList<RaisedQuestion> Questions) _ignoredWithAJob;
    (IReadOnlyList<(SuggestedEvent Suggestion, CodeJob? Job)> Suggestions, IReadOnlyList<RaisedQuestion> Questions) _free;

    void Because()
    {
        _job = NoCommandSuggestions.From(_uncaptured, [Answer("Reminders", UncorrelatedAnswers.ScheduledJob)]);
        _correction = NoCommandSuggestions.From(_uncaptured, [Answer("Reminders", UncorrelatedAnswers.StaffCorrection)]);
        _ignored = NoCommandSuggestions.From(_uncaptured, [Answer("Reminders", UncorrelatedAnswers.Ignore)]);
        _ignoredWithAJob = NoCommandSuggestions.From(_uncaptured, [Answer("Inventory", "Ignore")]);
        _free = NoCommandSuggestions.From(_uncaptured, [Answer("Reminders", "The cron on the old server")]);
    }

    SuggestedEvent Reminder((IReadOnlyList<(SuggestedEvent Suggestion, CodeJob? Job)> Suggestions, IReadOnlyList<RaisedQuestion> Questions) result) =>
        result.Suggestions.Select(item => item.Suggestion).Single(suggestion => suggestion.Stream.Value == "Reminder");

    [Fact] void should_stop_asking_once_it_is_answered() => _job.Questions.ShouldBeEmpty();
    [Fact] void should_suggest_an_event_for_a_scheduled_job() => Reminder(_job).Key.Value.ShouldEqual("Background/ReminderUpdated");
    [Fact] void should_say_no_code_explains_it() => Reminder(_job).Notes.Single().Text.ShouldContain("No code explains it. You answered: A scheduled job.");
    [Fact] void should_have_no_job_to_tie_it_to() => _job.Suggestions.Single(item => item.Suggestion.Stream.Value == "Reminder").Job.ShouldBeNull();
    [Fact] void should_suggest_a_correction_for_staff_correcting_data() => Reminder(_correction).Mechanical.Value.ShouldEqual("ReminderCorrected");
    [Fact] void should_say_a_correction_command_would_own_it() => Reminder(_correction).Notes.Single().Text.ShouldContain("correction command");
    [Fact] void should_suggest_nothing_when_told_to_ignore() => _ignored.Suggestions.Any(item => item.Suggestion.Stream.Value == "Reminder").ShouldBeFalse();
    [Fact] void should_not_ask_again_when_told_to_ignore() => _ignored.Questions.ShouldBeEmpty();
    [Fact] void should_keep_ignoring_even_when_a_job_is_found_later() => _ignoredWithAJob.Suggestions.Any(item => item.Suggestion.Stream.Value == "Inventory").ShouldBeFalse();
    [Fact] void should_take_free_text_as_what_the_writes_are() => Reminder(_free).Notes.Single().Text.ShouldContain("The cron on the old server");
    [Fact] void should_keep_the_key_when_the_numbers_in_the_question_change() => UncorrelatedQuestions.For(Written("Reminders", 40, 5)).Key.ShouldEqual(UncorrelatedQuestions.For(Written("Reminders", 4, 3)).Key);
    [Fact] void should_keep_the_identity_when_the_numbers_in_the_question_change() => UncorrelatedQuestions.For(Written("Reminders", 40, 5)).Id.ShouldEqual(UncorrelatedQuestions.For(Written("Reminders", 4, 3)).Id);
    [Fact] void should_say_the_writes_are_spread_when_no_hour_dominates() => UncorrelatedQuestions.When(new UncorrelatedTable(Written("Reminders", 6, 1).Write, 6, [new HourCount(1, 2), new HourCount(5, 2), new HourCount(9, 2)])).ShouldEqual(", spread across the day");
}
#endif
