// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_starting_with_a_restored_state : given.all_dependencies
{
    InterpreterSessionState _restored;

    void Establish()
    {
        _restored = StateWith(InterpreterStatus.AwaitingAnswers, new InterpreterQuestion(Guid.NewGuid(), "?", [], string.Empty));
        _host.Restore(_restored);
        SessionProceedsTo(_restored);
    }

    async Task Because()
    {
        await _host.Start(_llmOptions);
        await _host.Work;
    }

    [Fact] void should_resume_from_the_restored_state() => _sessions.Received(1).Resume(_restored, _captures, _llmOptions, Arg.Any<Action<InterpreterStatus>>(), Arg.Any<int>());
    [Fact] void should_not_create_a_new_session() => _sessions.DidNotReceive().CreateNew(Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Capture>>(), Arg.Any<LlmOptions>(), Arg.Any<Action<InterpreterStatus>>(), Arg.Any<int>());
}
#endif
