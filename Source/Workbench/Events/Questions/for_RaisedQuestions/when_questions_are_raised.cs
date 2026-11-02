// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Events.Uncaptured;

namespace Cratis.Prologue.Workbench.Events.Questions.for_RaisedQuestions;

public class when_questions_are_raised : an_uncaptured_library
{
    EventReview _raised;
    EventReview _answered;
    RaisedQuestion _reminders;

    void Because()
    {
        (_, _, var questions) = UncapturedEvents.Extend(_suggestions, _model, _uncaptured, []);
        _reminders = questions.Single();
        _raised = EventReview.Reduce(new RaisedQuestionsRecorded(questions), _review, _context);
        _answered = EventReview.Reduce(new InterpreterQuestionAnswered(_reminders.Key, _reminders.Prompt, "Ignore"), _raised, _context);
    }

    [Fact] void should_keep_the_questions_in_the_review() => _raised.Raised.Count().ShouldEqual(1);
    [Fact] void should_list_a_raised_question_as_open() => OpenQuestion.From([], _raised).Single().Prompt.ShouldEqual(_reminders.Prompt);
    [Fact] void should_say_no_interpretation_waits_for_it() => OpenQuestion.From([], _raised).Single().Standalone.ShouldBeTrue();
    [Fact] void should_identify_it_by_the_same_id_every_time() => RaisedQuestions.Find(_raised, _reminders.Id).ShouldNotBeNull();
    [Fact] void should_not_find_a_question_that_was_not_raised() => RaisedQuestions.Find(_raised, Guid.NewGuid()).ShouldBeNull();
    [Fact] void should_no_longer_list_it_once_answered() => OpenQuestion.From([], _answered).ShouldBeEmpty();
    [Fact] void should_no_longer_find_it_once_answered() => RaisedQuestions.Find(_answered, _reminders.Id).ShouldBeNull();
    [Fact] void should_keep_the_answer() => _answered.Answers.Single().Answer.ShouldEqual("Ignore");
    [Fact] void should_replace_the_raised_questions_with_the_next_interpretation() => EventReview.Reduce(new RaisedQuestionsRecorded([]), _raised, _context).Raised.ShouldBeEmpty();
}
#endif
