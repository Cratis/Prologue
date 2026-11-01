// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Scenarios.Scripting;

/// <summary>
/// Adds a scenario to the script of a prologue.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Name">The scenario, for example Lend a copy.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record AddScenario(PrologueId Prologue, ScenarioName Name)
{
    /// <summary>
    /// Handles the command, refusing a blank name and a scenario the script already holds.
    /// </summary>
    /// <param name="script">The script, read under the command's consistency boundary.</param>
    /// <returns>The scenario added, or a refusal.</returns>
    public Result<ScenarioAdded, ValidationResult> Handle(ScenarioScript? script)
    {
        if (ScenarioRules.RefuseBlank(Name) is { } blank)
        {
            return blank;
        }

        return ScenarioRules.Holds(script, ScenarioRules.Normalize(Name))
            ? ValidationResult.Error("The script already has this scenario")
            : new ScenarioAdded(ScenarioRules.Normalize(Name));
    }
}

/// <summary>
/// A scenario was added to the script of a prologue.
/// </summary>
/// <param name="Name">The scenario.</param>
[EventType]
public record ScenarioAdded(ScenarioName Name);
