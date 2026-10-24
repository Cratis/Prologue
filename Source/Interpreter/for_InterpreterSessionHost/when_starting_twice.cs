// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_starting_twice : given.all_dependencies
{
    void Establish() => SessionProceedsTo(StateWith(InterpreterStatus.AwaitingAnswers, new InterpreterQuestion(Guid.NewGuid(), "?", [], string.Empty)));

    async Task Because()
    {
        await _host.Start(_llmOptions);
        await _host.Work;
        await _host.Start(_llmOptions);
        await _host.Work;
    }

    [Fact] void should_create_only_one_session() => _sessions.Received(1).CreateNew(Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Capture>>(), Arg.Any<LlmOptions>(), Arg.Any<Action<InterpreterStatus>>(), Arg.Any<int>());
    [Fact] void should_read_the_captures_once() => _captureStore.Received(1).GetForPrologue(_prologueId, Arg.Any<CancellationToken>());
}
#endif
