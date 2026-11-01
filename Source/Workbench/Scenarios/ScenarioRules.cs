// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Scenarios;

/// <summary>
/// The rules about a prologue's scenario script, shared by the commands that change it.
/// </summary>
public static class ScenarioRules
{
    /// <summary>
    /// Gets the name a scenario is kept under: the name without the spaces around it.
    /// </summary>
    /// <param name="name">The name as typed.</param>
    /// <returns>The name to keep.</returns>
    public static ScenarioName Normalize(ScenarioName name) => new(name.Value.Trim());

    /// <summary>
    /// Gets why a name cannot be used for a scenario, when it cannot.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The refusal, or null when the name can be used.</returns>
    public static ValidationResult? RefuseBlank(ScenarioName name) =>
        string.IsNullOrWhiteSpace(name.Value) ? ValidationResult.Error("Pick or type a scenario first") : null;

    /// <summary>
    /// Gets whether a script already holds a scenario. Names are compared without regard to case.
    /// </summary>
    /// <param name="script">The script, or null when none exists yet.</param>
    /// <param name="name">The name.</param>
    /// <returns>True when the script holds it.</returns>
    public static bool Holds(ScenarioScript? script, ScenarioName name) =>
        script?.Scenarios.Any(held => string.Equals(held.Value, name.Value, StringComparison.OrdinalIgnoreCase)) ?? false;

    /// <summary>
    /// Gets the scenario that is running, if one is.
    /// </summary>
    /// <param name="script">The script, or null when none exists yet.</param>
    /// <returns>The running scenario, or <see cref="ScenarioName.None"/>.</returns>
    public static ScenarioName Running(ScenarioScript? script) => script?.Active ?? ScenarioName.None;
}
