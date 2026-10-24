// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionStateDocument;

public class when_converting_an_empty_document : Specification
{
    InterpreterSessionStateDocument _document;
    InterpreterSessionState? _result;

    void Establish() => _document = new InterpreterSessionStateDocument();

    void Because() => _result = _document.ToState();

    [Fact] void should_hold_no_state() => _result.ShouldBeNull();
}
#endif
