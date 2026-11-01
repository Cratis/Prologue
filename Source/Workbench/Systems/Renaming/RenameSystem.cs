// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Systems.Renaming;

/// <summary>
/// Renames a system, which changes its service name in the configuration.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Id">The system.</param>
/// <param name="Name">The new name.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record RenameSystem(PrologueId Prologue, SystemId Id, ServiceName Name)
{
    /// <summary>
    /// Handles the command, refusing an unknown system and a blank, repeated or unchanged name.
    /// </summary>
    /// <param name="prologue">The prologue, read under the command's consistency boundary.</param>
    /// <returns>The rename, or a refusal.</returns>
    public Result<SystemRenamed, ValidationResult> Handle(PrologueDefinition? prologue)
    {
        if (SystemRules.RefuseUnknown(prologue, Id) is { } unknown)
        {
            return unknown;
        }

        if (SystemRules.RefuseName(prologue, Name, Id) is { } refusal)
        {
            return refusal;
        }

        var name = new ServiceName(Name.Value.Trim());
        return SystemRules.Find(prologue, Id)!.Name == name ? ValidationResult.Error("The system already has this name") : new SystemRenamed(Id, name);
    }
}

/// <summary>
/// A system was renamed.
/// </summary>
/// <param name="Id">The system.</param>
/// <param name="Name">Its new service name.</param>
[EventType]
public record SystemRenamed(SystemId Id, ServiceName Name);
