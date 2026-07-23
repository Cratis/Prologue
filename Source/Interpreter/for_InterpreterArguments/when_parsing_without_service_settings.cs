// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_InterpreterArguments;

[Collection(nameof(InterpreterArguments))]
public class when_parsing_without_service_settings : given.clean_environment
{
    InterpreterArguments? _result;

    void Because() => _result = InterpreterArguments.Parse([]);

    [Fact] void should_not_serve() => _result!.Serve.ShouldBeFalse();
    [Fact] void should_use_the_default_service_port() => _result!.ServicePort.ShouldEqual(InterpreterArguments.DefaultServicePort);
    [Fact] void should_use_the_default_grace_period() => _result!.GracePeriod.ShouldEqual(TimeSpan.FromSeconds(InterpreterArguments.DefaultGracePeriodSeconds));
    [Fact] void should_use_the_default_idle_timeout() => _result!.IdleTimeout.ShouldEqual(TimeSpan.FromSeconds(InterpreterArguments.DefaultIdleTimeoutSeconds));
}
#endif
