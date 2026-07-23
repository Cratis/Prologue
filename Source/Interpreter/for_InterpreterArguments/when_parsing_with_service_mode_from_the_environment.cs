// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_InterpreterArguments;

[Collection(nameof(InterpreterArguments))]
public class when_parsing_with_service_mode_from_the_environment : given.clean_environment
{
    InterpreterArguments? _result;

    void Establish() => Environment.SetEnvironmentVariable("PROLOGUE_MODE", "Service");

    void Because() => _result = InterpreterArguments.Parse([]);

    [Fact] void should_serve_regardless_of_casing() => _result!.Serve.ShouldBeTrue();
}
#endif
