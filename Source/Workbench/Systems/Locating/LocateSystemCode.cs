// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Systems.Locating;

/// <summary>
/// Says where a system's code lives. Several systems can share a repository by pointing at different sub paths.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Id">The system.</param>
/// <param name="Host">The kind of host.</param>
/// <param name="Repository">The repository, or the folder for a local system.</param>
/// <param name="Branch">The branch.</param>
/// <param name="SubPath">The path inside the repository; empty when the system is the whole repository.</param>
[Command]
public record LocateSystemCode(PrologueId Prologue, SystemId Id, RepositoryHost Host, RepositoryName Repository, BranchName Branch, SubPath SubPath)
{
    /// <summary>
    /// Handles the command, refusing an unknown system, a blank repository and a location the system already has.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The location, or a refusal.</returns>
    public Result<SystemCodeLocated, ValidationResult> Handle(PrologueDefinition? prologue)
    {
        if (SystemRules.RefuseUnknown(prologue, Id) is { } unknown)
        {
            return unknown;
        }

        if (string.IsNullOrWhiteSpace(Repository.Value))
        {
            return ValidationResult.Error(Host == RepositoryHost.LocalFolder ? "Say which folder the code is in" : "Say which repository the code is in");
        }

        var located = new SystemCodeLocated(
            Id,
            Host,
            new RepositoryName(Repository.Value.Trim()),
            new BranchName(string.IsNullOrWhiteSpace(Branch.Value) ? "main" : Branch.Value.Trim()),
            new SubPath(SubPath.Value.Trim().Trim('/')));

        return SystemRules.Find(prologue, Id)!.Location == located.ToLocation()
            ? ValidationResult.Error("The system's code is already located here")
            : located;
    }
}

/// <summary>
/// Where a system's code lives was recorded.
/// </summary>
/// <param name="Id">The system.</param>
/// <param name="Host">The kind of host.</param>
/// <param name="Repository">The repository, or the folder for a local system.</param>
/// <param name="Branch">The branch.</param>
/// <param name="SubPath">The path inside the repository; empty for the whole repository.</param>
[EventType]
public record SystemCodeLocated(SystemId Id, RepositoryHost Host, RepositoryName Repository, BranchName Branch, SubPath SubPath)
{
    /// <summary>
    /// Gets the location the event records.
    /// </summary>
    /// <returns>The location.</returns>
    public SystemCodeLocation ToLocation() => new(Host, Repository, Branch, SubPath);
}
