// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_InterpreterArguments;

[Collection(nameof(InterpreterArguments))]
public class when_parsing_with_the_serve_argument : given.clean_environment
{
    InterpreterArguments? _result;

    void Because() => _result = InterpreterArguments.Parse(["--serve"]);

    [Fact] void should_serve() => _result!.Serve.ShouldBeTrue();
    [Fact] void should_keep_the_default_captures_folder() => _result!.CapturesFolder.ShouldEqual(InterpreterArguments.DefaultCapturesFolder);
}
#endif
