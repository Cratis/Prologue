// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The rules every command that changes a system shares.
/// </summary>
public static class SystemRules
{
    /// <summary>
    /// Finds a system in a prologue.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="id">The system.</param>
    /// <returns>The system, or null when the prologue has no such system.</returns>
    public static PrologueSystem? Find(PrologueDefinition? prologue, SystemId id) =>
        prologue?.Systems.FirstOrDefault(system => system.Id == id);

    /// <summary>
    /// Gets why a system cannot be changed, when it cannot.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="id">The system.</param>
    /// <returns>The refusal, or null when the system exists.</returns>
    public static ValidationResult? RefuseUnknown(PrologueDefinition? prologue, SystemId id) => prologue switch
    {
        null => ValidationResult.Error("The prologue is not defined"),
        _ when Find(prologue, id) is null => ValidationResult.Error("The prologue has no such system"),
        _ => null
    };

    /// <summary>
    /// Gets why a name cannot be given to a system, when it cannot. A name is the system's service name, so it is
    /// not blank and unique within the prologue.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="name">The name.</param>
    /// <param name="renaming">The system being renamed, which may keep its own name.</param>
    /// <returns>The refusal, or null when the name can be used.</returns>
    public static ValidationResult? RefuseName(PrologueDefinition? prologue, ServiceName name, SystemId? renaming = null)
    {
        if (prologue is null)
        {
            return ValidationResult.Error("The prologue is not defined");
        }

        if (string.IsNullOrWhiteSpace(name.Value))
        {
            return ValidationResult.Error("Give the system a name");
        }

        return prologue.Systems.Any(system => system.Id != renaming && string.Equals(system.Name.Value, name.Value.Trim(), StringComparison.OrdinalIgnoreCase))
            ? ValidationResult.Error($"The prologue already has a system called {name.Value.Trim()}")
            : null;
    }

    /// <summary>
    /// Gets a list of values without blanks and repeats, in the order given.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The distinct, non-blank values.</returns>
    public static IEnumerable<string> Clean(IEnumerable<string> values) =>
        [.. values.Select(value => value.Trim()).Where(value => value.Length > 0).Distinct(StringComparer.Ordinal)];
}
