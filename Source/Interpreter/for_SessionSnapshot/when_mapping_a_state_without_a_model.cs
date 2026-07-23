// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_SessionSnapshot;

public class when_mapping_a_state_without_a_model : Specification
{
    static readonly Guid _prologueId = new("11111111-2222-3333-4444-555555555555");

    InterpreterSessionState _state;
    SessionSnapshot _result;

    void Establish() => _state = InterpreterSessionState.New(_prologueId) with { Status = InterpreterStatus.Failed, Error = "it broke" };

    void Because() => _result = SessionSnapshot.From(_state);

    [Fact] void should_carry_the_status() => _result.Status.ShouldEqual(InterpreterStatus.Failed);
    [Fact] void should_carry_an_empty_system_name() => _result.SystemName.ShouldEqual(string.Empty);
    [Fact] void should_carry_the_error() => _result.Error.ShouldEqual("it broke");
}
#endif
