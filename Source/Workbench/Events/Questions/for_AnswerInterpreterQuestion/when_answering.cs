// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Questions.for_AnswerInterpreterQuestion;

public class when_answering : Specification
{
    static readonly InterpreterQuestion _first = new(Guid.NewGuid(), "Is fulfilling a reservation a fact?", [new QuestionChoice("Yes", "Record it"), new QuestionChoice("No", "It is cleanup")], "The reservation row is deleted with the loan");
    static readonly InterpreterQuestion _second = new(Guid.NewGuid(), "Are stocking and adding copies the same?", [], "One endpoint inserts and updates");
    PrologueId _prologue;
    InterpretationSessions _sessions;
    IInterpretationQueue _queue;
    Result<InterpreterQuestionAnswered, ValidationResult> _firstAnswer;
    Result<InterpreterQuestionAnswered, ValidationResult> _secondAnswer;
    Result<InterpreterQuestionAnswered, ValidationResult> _again;
    Result<InterpreterQuestionAnswered, ValidationResult> _blank;
    Result<InterpreterQuestionAnswered, ValidationResult> _unknown;

    void Establish()
    {
        _prologue = PrologueId.New();
        _sessions = new InterpretationSessions();
        _queue = Substitute.For<IInterpretationQueue>();
        var state = InterpreterSessionState.New(_prologue.Value) with { Status = InterpreterStatus.AwaitingAnswers, PendingQuestions = [_first, _second] };
        _sessions.Park(_prologue, new ParkedInterpretation(state, new InterpretationContext([], [], new Cratis.Prologue.Interpreter.Contracts.ExtractionResult(_prologue.Value, []), EvidenceFacts.None, null, [])));
    }

    void Because()
    {
        _blank = new AnswerInterpreterQuestion(_prologue, _first.Id, "  ").Handle(_sessions, _queue);
        _unknown = new AnswerInterpreterQuestion(_prologue, Guid.NewGuid(), "Yes").Handle(_sessions, _queue);
        _firstAnswer = new AnswerInterpreterQuestion(_prologue, _first.Id, " Yes ").Handle(_sessions, _queue);
        var queuedAfterTheFirst = _queue.ReceivedCalls().Count();
        _again = new AnswerInterpreterQuestion(_prologue, _first.Id, "Yes").Handle(_sessions, _queue);
        _secondAnswer = new AnswerInterpreterQuestion(_prologue, _second.Id, "They are the same thing").Handle(_sessions, _queue);
        _queuedBeforeTheLast = queuedAfterTheFirst;
    }

    int _queuedBeforeTheLast;

    [Fact] void should_refuse_a_blank_answer() => _blank.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_refuse_a_question_that_is_not_waiting() => _unknown.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_record_the_answer_trimmed() => _firstAnswer.Appended()!.Answer.ShouldEqual("Yes");
    [Fact] void should_record_the_question_with_the_answer() => _firstAnswer.Appended()!.Prompt.ShouldEqual(_first.Prompt);
    [Fact] void should_key_the_question_by_its_prompt() => _firstAnswer.Appended()!.Key.ShouldEqual(QuestionKeys.For(_first.Prompt));
    [Fact] void should_not_carry_on_while_a_question_is_left() => _queuedBeforeTheLast.ShouldEqual(0);
    [Fact] void should_refuse_answering_a_question_twice() => _again.TryGetError(out _).ShouldBeTrue();
    [Fact] void should_carry_on_once_every_question_is_answered() => _queue.Received(1).Enqueue(Arg.Is<InterpretationJob>(job => job.Prologue == _prologue && job.Resume));
    [Fact] void should_accept_free_text() => _secondAnswer.Appended()!.Answer.ShouldEqual("They are the same thing");
    [Fact] void should_leave_nothing_waiting() => _sessions.Pending(_prologue).ShouldBeEmpty();
}
#endif
