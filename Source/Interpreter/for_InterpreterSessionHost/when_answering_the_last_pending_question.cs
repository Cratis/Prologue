// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_answering_the_last_pending_question : given.all_dependencies
{
    InterpreterQuestion _question;
    InterpreterAnswer _answer;
    InterpreterSessionState _answered;

    void Establish()
    {
        _question = new InterpreterQuestion(Guid.NewGuid(), "Is an author the same as a writer?", [], string.Empty);
        SessionProceedsTo(StateWith(InterpreterStatus.AwaitingAnswers, _question));
        _answered = StateWith(InterpreterStatus.AwaitingAnswers);
        _answer = new InterpreterAnswer(_question.Id, "Yes");
        _session.Answer(_answer).Returns(_answered);
    }

    async Task Because()
    {
        await _host.Start(_llmOptions);
        await _host.Work;
        await _host.Answer(_answer);
        await _host.Work;
    }

    [Fact] void should_record_the_answer() => _session.Received(1).Answer(_answer);
    [Fact] void should_report_the_answer_to_the_activity_tracker() => _activity.Received(1).AnswerReceived();
    [Fact] void should_persist_the_answered_state() => _persisted.ShouldContain(_answered);
    [Fact] void should_continue_the_session() => _session.Received(2).Proceed(Arg.Any<CancellationToken>());
}
#endif
