// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Contracts;

namespace Cratis.Prologue.Interpreter.for_InterpreterArguments;

[Collection(nameof(InterpreterArguments))]
public class when_parsing_with_a_captured_service : given.clean_environment
{
    InterpreterArguments? _result;

    void Because() => _result = InterpreterArguments.Parse(["--captured-service", "billing"]);

    [Fact] void should_interpret_only_that_service() => _result!.CapturedService.ShouldEqual(new ServiceName("billing"));
    [Fact] void should_stay_a_batch_run() => _result!.Serve.ShouldBeFalse();
}
#endif
