// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Events.Uncaptured.for_UncorrelatedAnswers;

public class when_reading_what_an_answer_means : Specification
{
    [Fact] void should_read_the_job_choice_as_a_job() => UncorrelatedAnswers.Mean(UncorrelatedAnswers.ScheduledJob).ShouldEqual(UncorrelatedMeaning.Job);
    [Fact] void should_read_the_correction_choice_as_a_correction() => UncorrelatedAnswers.Mean(UncorrelatedAnswers.StaffCorrection).ShouldEqual(UncorrelatedMeaning.Correction);
    [Fact] void should_read_the_ignore_choice_as_ignored() => UncorrelatedAnswers.Mean(UncorrelatedAnswers.Ignore).ShouldEqual(UncorrelatedMeaning.Ignored);
    [Fact] void should_read_free_text_as_a_job() => UncorrelatedAnswers.Mean("The cron on the old server").ShouldEqual(UncorrelatedMeaning.Job);
    [Fact] void should_not_take_a_negated_correction_for_one() => UncorrelatedAnswers.Mean("That's not correct, it's a job").ShouldEqual(UncorrelatedMeaning.Job);
    [Fact] void should_not_take_the_word_in_passing_for_a_correction() => UncorrelatedAnswers.Mean("Nightly sync that keeps the totals correct").ShouldEqual(UncorrelatedMeaning.Job);
    [Fact] void should_not_take_a_word_that_only_contains_it_for_one() => UncorrelatedAnswers.Mean("An incorrectly scheduled import").ShouldEqual(UncorrelatedMeaning.Job);
    [Fact] void should_take_a_clear_description_of_staff_correcting_data() => UncorrelatedAnswers.Mean("Staff correct this data by hand").ShouldEqual(UncorrelatedMeaning.Correction);
    [Fact] void should_take_a_plain_correction() => UncorrelatedAnswers.Mean("Manual corrections").ShouldEqual(UncorrelatedMeaning.Correction);
    [Fact] void should_read_free_text_that_starts_with_ignore_as_ignored() => UncorrelatedAnswers.Mean("Ignore these, test data").ShouldEqual(UncorrelatedMeaning.Ignored);
}
#endif
