// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Staging;

/// <summary>
/// Starts shadowing a slice: its legacy commands are mirrored to the new slice and the decisions compared.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Name">The name of the slice.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record StartShadowing(PrologueId Prologue, RebuildSliceName Name)
{
    /// <summary>
    /// Handles the command, refusing a slice that is not built.
    /// </summary>
    /// <param name="board">The board, read under the command's consistency boundary.</param>
    /// <returns>The shadowing started, or a refusal.</returns>
    public Result<ShadowingStarted, ValidationResult> Handle(RebuildBoard? board) =>
        RebuildRules.RefuseMoveFrom(board, Name, RebuildStage.Built) is { } refusal
            ? refusal
            : new ShadowingStarted(RebuildRules.Find(board, Name)!.Name);
}

/// <summary>
/// Shadowing of a slice started.
/// </summary>
/// <param name="Name">The name of the slice.</param>
[EventType]
public record ShadowingStarted(RebuildSliceName Name);
