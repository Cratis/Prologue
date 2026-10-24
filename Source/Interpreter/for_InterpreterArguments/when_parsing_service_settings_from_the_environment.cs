// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_InterpreterArguments;

[Collection(nameof(InterpreterArguments))]
public class when_parsing_service_settings_from_the_environment : given.clean_environment
{
    InterpreterArguments? _result;

    void Establish()
    {
        Environment.SetEnvironmentVariable("PROLOGUE_SERVICE_PORT", "8080");
        Environment.SetEnvironmentVariable("PROLOGUE_GRACE_PERIOD", "60");
        Environment.SetEnvironmentVariable("PROLOGUE_IDLE_TIMEOUT", "120");
    }

    void Because() => _result = InterpreterArguments.Parse([]);

    [Fact] void should_use_the_port_from_the_environment() => _result!.ServicePort.ShouldEqual(8080);
    [Fact] void should_use_the_grace_period_from_the_environment() => _result!.GracePeriod.ShouldEqual(TimeSpan.FromSeconds(60));
    [Fact] void should_use_the_idle_timeout_from_the_environment() => _result!.IdleTimeout.ShouldEqual(TimeSpan.FromSeconds(120));
}
#endif
