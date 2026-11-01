// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Scenarios.Scripting;

/// <summary>
/// Removes a scenario from the script of a prologue.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Name">The scenario.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record RemoveScenario(PrologueId Prologue, ScenarioName Name)
{
    /// <summary>
    /// Handles the command, refusing a scenario the script does not hold and the scenario that is running.
    /// </summary>
    /// <param name="script">The script, read under the command's consistency boundary.</param>
    /// <returns>The scenario removed, or a refusal.</returns>
    public Result<ScenarioRemoved, ValidationResult> Handle(ScenarioScript? script)
    {
        if (!ScenarioRules.Holds(script, ScenarioRules.Normalize(Name)))
        {
            return ValidationResult.Error("The script has no such scenario");
        }

        return ScenarioRules.Running(script) == ScenarioRules.Normalize(Name)
            ? ValidationResult.Error("This scenario is running. Mark it ended first")
            : new ScenarioRemoved(script!.Scenarios.First(held => string.Equals(held.Value, Name.Value.Trim(), StringComparison.OrdinalIgnoreCase)));
    }
}

/// <summary>
/// A scenario was removed from the script of a prologue.
/// </summary>
/// <param name="Name">The scenario.</param>
[EventType]
public record ScenarioRemoved(ScenarioName Name);
