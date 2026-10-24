// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_answering_an_unknown_question : given.all_dependencies
{
    InterpreterAnswer _answer;
    Exception _error;

    void Establish()
    {
        SessionProceedsTo(StateWith(InterpreterStatus.AwaitingAnswers, new InterpreterQuestion(Guid.NewGuid(), "?", [], string.Empty)));
        _answer = new InterpreterAnswer(Guid.NewGuid(), "Yes");
        _session.Answer(_answer).Returns(_ => throw new QuestionNotPending(_answer.QuestionId));
    }

    async Task Because()
    {
        await _host.Start(_llmOptions);
        await _host.Work;
        _error = await Catch.Exception(() => _host.Answer(_answer));
    }

    [Fact] void should_fail_with_question_not_pending() => _error.ShouldBeOfExactType<QuestionNotPending>();
    [Fact] void should_not_persist_anything_beyond_the_checkpoint() => _persisted.Count.ShouldEqual(1);
}
#endif
