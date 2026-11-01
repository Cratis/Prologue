// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios;

namespace Cratis.Prologue.Workbench.Scenarios.Marking;

/// <summary>
/// Marks the start of a scenario: captures that arrive for the prologue from now until the scenario is marked ended carry its name.
/// Only one scenario runs at a time per prologue.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Name">The scenario, picked from the script or typed.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record MarkScenarioStarted(PrologueId Prologue, ScenarioName Name)
{
    /// <summary>
    /// Handles the command, refusing a blank name and a start while another scenario, or this one, is still running.
    /// </summary>
    /// <param name="script">The script, read under the command's consistency boundary so two starts cannot both win.</param>
    /// <returns>The scenario started, or a refusal.</returns>
    public Result<ScenarioStarted, ValidationResult> Handle(ScenarioScript? script)
    {
        if (ScenarioRules.RefuseBlank(Name) is { } blank)
        {
            return blank;
        }

        var running = ScenarioRules.Running(script);
        return running != ScenarioName.None
            ? ValidationResult.Error($"\"{running.Value}\" is still running. Mark it ended before starting another scenario")
            : new ScenarioStarted(ScenarioRules.Normalize(Name));
    }
}

/// <summary>
/// A scenario started: captures arriving for the prologue are labeled with it until it ends.
/// </summary>
/// <param name="Name">The scenario.</param>
[EventType]
public record ScenarioStarted(ScenarioName Name);
