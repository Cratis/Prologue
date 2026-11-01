// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Systems.Removing;

/// <summary>
/// Removes a system from a prologue.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Id">The system.</param>
[Command]
public record RemoveSystem(PrologueId Prologue, SystemId Id)
{
    /// <summary>
    /// Handles the command, refusing an unknown system.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The removal, or a refusal.</returns>
    public Result<SystemRemoved, ValidationResult> Handle(PrologueDefinition? prologue) =>
        SystemRules.RefuseUnknown(prologue, Id) is { } refusal ? refusal : new SystemRemoved(Id);
}

/// <summary>
/// A system was removed from the prologue.
/// </summary>
/// <param name="Id">The system.</param>
[EventType]
public record SystemRemoved(SystemId Id);
