// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_starting_a_new_session : given.all_dependencies
{
    InterpreterSessionState _completed;

    void Establish()
    {
        _completed = StateWith(InterpreterStatus.Completed) with { Model = new ExtractionResult(_prologueId, [], "LibrarySystem") };
        SessionProceedsTo(_completed);
        _screenplays.Generate(Arg.Any<ExtractionResult>()).Returns("module Library");
    }

    async Task Because()
    {
        await _host.Start(_llmOptions);
        await _host.Work;
    }

    [Fact] void should_create_the_session_from_the_stored_captures() => _sessions.Received(1).CreateNew(_prologueId, _captures, _llmOptions, Arg.Any<Action<InterpreterStatus>>(), Arg.Any<int>());
    [Fact] void should_touch_the_activity_tracker() => _activity.Received(1).SessionTouched();
    [Fact] void should_persist_the_checkpointed_state() => _persisted.Single().ShouldEqual(_completed);
    [Fact] void should_generate_the_screenplay_when_completing() => _screenplays.Received(1).Generate(_completed.Model!);
}
#endif
