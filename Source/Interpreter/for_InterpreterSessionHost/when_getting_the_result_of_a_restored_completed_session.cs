// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost;

public class when_getting_the_result_of_a_restored_completed_session : given.all_dependencies
{
    ExtractionResult _model;
    SessionResult? _result;

    void Establish()
    {
        _model = new ExtractionResult(_prologueId, [], "LibrarySystem");
        _host.Restore(StateWith(InterpreterStatus.Completed) with { Model = _model });
        _screenplays.Generate(_model).Returns("module Library");
    }

    void Because() => _result = _host.GetResult();

    [Fact] void should_regenerate_the_screenplay_from_the_persisted_model() => _result!.Screenplay.ShouldEqual("module Library");
    [Fact] void should_carry_the_extraction_result() => _result!.ExtractionResult.ShouldEqual(_model);
}
#endif
