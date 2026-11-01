// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Scenarios.Marking;

/// <summary>
/// Marks the end of the scenario that is running.
/// </summary>
/// <param name="Prologue">The prologue.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record MarkScenarioEnded(PrologueId Prologue)
{
    /// <summary>
    /// Handles the command, refusing an end when no scenario is running.
    /// </summary>
    /// <param name="script">The script, read under the command's consistency boundary.</param>
    /// <returns>The scenario ended, or a refusal.</returns>
    public Result<ScenarioEnded, ValidationResult> Handle(ScenarioScript? script)
    {
        var running = ScenarioRules.Running(script);
        return running == ScenarioName.None
            ? ValidationResult.Error("No scenario is running")
            : new ScenarioEnded(running);
    }
}

/// <summary>
/// A scenario ended: captures arriving for the prologue are no longer labeled with it.
/// </summary>
/// <param name="Name">The scenario.</param>
[EventType]
public record ScenarioEnded(ScenarioName Name);
