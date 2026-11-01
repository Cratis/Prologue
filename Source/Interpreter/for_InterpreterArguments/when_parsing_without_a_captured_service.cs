// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_InterpreterArguments;

[Collection(nameof(InterpreterArguments))]
public class when_parsing_without_a_captured_service : given.clean_environment
{
    InterpreterArguments? _result;

    void Because() => _result = InterpreterArguments.Parse([]);

    [Fact] void should_interpret_every_service() => _result!.CapturedService.ShouldBeNull();
}
#endif
