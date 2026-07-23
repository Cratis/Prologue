// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_getting_the_result_after_completion : given.all_dependencies
{
    ExtractionResult _model;
    SessionResult? _result;

    void Establish()
    {
        _model = new ExtractionResult(_prologueId, [], "LibrarySystem");
        SessionProceedsTo(StateWith(InterpreterStatus.Completed) with { Model = _model });
        _screenplays.Generate(_model).Returns("module Library");
    }

    async Task Because()
    {
        await _host.Start(_llmOptions);
        await _host.Work;
        _result = _host.GetResult();
    }

    [Fact] void should_carry_the_extraction_result() => _result!.ExtractionResult.ShouldEqual(_model);
    [Fact] void should_carry_the_screenplay() => _result!.Screenplay.ShouldEqual("module Library");
    [Fact] void should_generate_the_screenplay_once() => _screenplays.Received(1).Generate(_model);
    [Fact] void should_count_the_fetch() => _activity.Received(1).ResultFetched();
}
#endif
