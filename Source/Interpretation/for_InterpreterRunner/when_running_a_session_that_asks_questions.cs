// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_InterpreterRunner;

public class when_running_a_session_that_asks_questions : Specification
{
    static readonly Guid _prologueId = new("11111111-2222-3333-4444-555555555555");

    IInterpreterSession _session;
    IInterpreterCallbacks _callbacks;
    InterpreterRunner _runner;
    InterpreterQuestion _first;
    InterpreterQuestion _second;
    InterpreterAnswer _firstAnswer;
    InterpreterAnswer _secondAnswer;
    InterpreterSessionState _result;

    void Establish()
    {
        _first = new InterpreterQuestion(Guid.NewGuid(), "First question?", [], "First context");
        _second = new InterpreterQuestion(Guid.NewGuid(), "Second question?", [], "Second context");
        _firstAnswer = new InterpreterAnswer(_first.Id, "First answer");
        _secondAnswer = new InterpreterAnswer(_second.Id, "Second answer");

        var initial = InterpreterSessionState.New(_prologueId);
        var awaitingBoth = initial with { Status = InterpreterStatus.AwaitingAnswers, PendingQuestions = [_first, _second] };
        var awaitingSecond = awaitingBoth with { PendingQuestions = [_second] };
        var allAnswered = awaitingBoth with { PendingQuestions = [] };
        var completed = initial with { Status = InterpreterStatus.Completed };

        _session = Substitute.For<IInterpreterSession>();
        _session.State.Returns(initial);
        _session.Proceed(Arg.Any<CancellationToken>()).Returns(awaitingBoth, completed);
        _session.Answer(_firstAnswer).Returns(awaitingSecond);
        _session.Answer(_secondAnswer).Returns(allAnswered);

        _callbacks = Substitute.For<IInterpreterCallbacks>();
        _callbacks.OnQuestion(_first).Returns(_firstAnswer);
        _callbacks.OnQuestion(_second).Returns(_secondAnswer);

        _runner = new InterpreterRunner();
    }

    async Task Because() => _result = await _runner.Run(_session, _callbacks);

    [Fact] void should_return_the_completed_state() => _result.Status.ShouldEqual(InterpreterStatus.Completed);
    [Fact] void should_ask_each_question_exactly_once() => _callbacks.Received(2).OnQuestion(Arg.Any<InterpreterQuestion>());
    [Fact] void should_proceed_again_after_all_answers() => _session.Received(2).Proceed(Arg.Any<CancellationToken>());

    [Fact]
    void should_surface_the_questions_one_at_a_time()
    {
        Received.InOrder(() =>
        {
            _callbacks.OnQuestion(_first);
            _session.Answer(_firstAnswer);
            _callbacks.OnQuestion(_second);
            _session.Answer(_secondAnswer);
        });
    }
}
#endif
