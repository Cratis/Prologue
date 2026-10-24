// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_answering_with_questions_still_pending : given.all_dependencies
{
    InterpreterQuestion _first;
    InterpreterQuestion _second;
    InterpreterAnswer _answer;

    void Establish()
    {
        _first = new InterpreterQuestion(Guid.NewGuid(), "First?", [], string.Empty);
        _second = new InterpreterQuestion(Guid.NewGuid(), "Second?", [], string.Empty);
        SessionProceedsTo(StateWith(InterpreterStatus.AwaitingAnswers, _first, _second));
        _answer = new InterpreterAnswer(_first.Id, "Yes");
        _session.Answer(_answer).Returns(StateWith(InterpreterStatus.AwaitingAnswers, _second));
    }

    async Task Because()
    {
        await _host.Start(_llmOptions);
        await _host.Work;
        await _host.Answer(_answer);
        await _host.Work;
    }

    [Fact] void should_record_the_answer() => _session.Received(1).Answer(_answer);
    [Fact] void should_not_continue_until_every_question_is_answered() => _session.Received(1).Proceed(Arg.Any<CancellationToken>());
}
#endif
