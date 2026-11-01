// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Contracts;

namespace Cratis.Prologue.Interpreter.for_InterpreterArguments;

[Collection(nameof(InterpreterArguments))]
public class when_parsing_with_a_captured_service_from_the_environment : given.clean_environment
{
    InterpreterArguments? _result;

    void Establish() => Environment.SetEnvironmentVariable("PROLOGUE_CAPTURED_SERVICE", "orders");

    void Because() => _result = InterpreterArguments.Parse([]);

    [Fact] void should_interpret_only_that_service() => _result!.CapturedService.ShouldEqual(new ServiceName("orders"));
}
#endif
