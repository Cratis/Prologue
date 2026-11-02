// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Events.Uncaptured;

namespace Cratis.Prologue.Workbench.Events.Questions.for_AnswerInterpreterQuestion;

public class when_answering_a_raised_question : an_uncaptured_library
{
    Result<InterpreterQuestionAnswered, ValidationResult> _answer;
    Result<InterpreterQuestionAnswered, ValidationResult> _unknown;
    RaisedQuestion _raised;

    void Because()
    {
        (_, _, var questions) = UncapturedEvents.Extend(_suggestions, _model, _uncaptured, []);
        _raised = questions.Single();
        var review = EventReview.Reduce(new RaisedQuestionsRecorded(questions), _review, _context);
        var sessions = new InterpretationSessions();
        var queue = Substitute.For<IInterpretationQueue>();
        _answer = new AnswerInterpreterQuestion(_prologue, _raised.Id, "A scheduled job").Handle(sessions, queue, review);
        _unknown = new AnswerInterpreterQuestion(_prologue, Guid.NewGuid(), "A scheduled job").Handle(sessions, queue, review);
    }

    [Fact] void should_record_the_answer_by_the_key_of_the_raised_question() => _answer.Appended()!.Key.ShouldEqual(_raised.Key);
    [Fact] void should_record_the_prompt_it_answers() => _answer.Appended()!.Prompt.ShouldEqual(_raised.Prompt);
    [Fact] void should_record_the_answer() => _answer.Appended()!.Answer.ShouldEqual("A scheduled job");
    [Fact] void should_refuse_a_question_nobody_raised() => _unknown.TryGetError(out _).ShouldBeTrue();
}
#endif
