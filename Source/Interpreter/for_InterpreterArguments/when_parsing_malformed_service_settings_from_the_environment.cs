// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_InterpreterArguments;

[Collection(nameof(InterpreterArguments))]
public class when_parsing_malformed_service_settings_from_the_environment : given.clean_environment
{
    InterpreterArguments? _result;

    void Establish()
    {
        Environment.SetEnvironmentVariable("PROLOGUE_SERVICE_PORT", "not-a-number");
        Environment.SetEnvironmentVariable("PROLOGUE_GRACE_PERIOD", "-5");
        Environment.SetEnvironmentVariable("PROLOGUE_IDLE_TIMEOUT", "0");
    }

    void Because() => _result = InterpreterArguments.Parse([]);

    [Fact] void should_fall_back_to_the_default_service_port() => _result!.ServicePort.ShouldEqual(InterpreterArguments.DefaultServicePort);
    [Fact] void should_fall_back_to_the_default_grace_period() => _result!.GracePeriodSeconds.ShouldEqual(InterpreterArguments.DefaultGracePeriodSeconds);
    [Fact] void should_fall_back_to_the_default_idle_timeout() => _result!.IdleTimeoutSeconds.ShouldEqual(InterpreterArguments.DefaultIdleTimeoutSeconds);
}
#endif
