// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Systems.Adding;

/// <summary>
/// Adds a system to a prologue. The name is the system's service name and is unique within the prologue.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Id">The identity of the new system; supplied by the caller so a retry reuses it.</param>
/// <param name="Name">The system's name.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record AddSystem(PrologueId Prologue, SystemId Id, ServiceName Name)
{
    /// <summary>
    /// Handles the command, refusing a blank or repeated name.
    /// </summary>
    /// <param name="prologue">The prologue, read under the command's consistency boundary.</param>
    /// <returns>The system added, or a refusal.</returns>
    public Result<SystemAdded, ValidationResult> Handle(PrologueDefinition? prologue) =>
        SystemRules.RefuseName(prologue, Name) is { } refusal ? refusal : new SystemAdded(Id, new ServiceName(Name.Value.Trim()));
}

/// <summary>
/// A system was added to the prologue.
/// </summary>
/// <param name="Id">The system.</param>
/// <param name="Name">Its service name.</param>
[EventType]
public record SystemAdded(SystemId Id, ServiceName Name);
