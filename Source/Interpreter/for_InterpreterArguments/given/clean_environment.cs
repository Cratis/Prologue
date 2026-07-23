// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_InterpreterArguments.given;

public class clean_environment : Specification
{
    static readonly string[] _variables =
    [
        "PROLOGUE_CAPTURES",
        "PROLOGUE_OUTPUT",
        "PROLOGUE_PLAY_OUTPUT",
        "PROLOGUE_ID",
        "PROLOGUE_MODE",
        "PROLOGUE_SERVICE_PORT",
        "PROLOGUE_GRACE_PERIOD",
        "PROLOGUE_IDLE_TIMEOUT"
    ];

    Dictionary<string, string?> _original;

    void Establish()
    {
        _original = _variables.ToDictionary(variable => variable, Environment.GetEnvironmentVariable);
        foreach (var variable in _variables)
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    void Destroy()
    {
        foreach (var (variable, value) in _original)
        {
            Environment.SetEnvironmentVariable(variable, value);
        }
    }
}
#endif
