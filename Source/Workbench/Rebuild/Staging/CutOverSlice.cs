// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Staging;

/// <summary>
/// Cuts a slice over: its route now goes to the new slice. Refused while the slice has differences nobody has resolved.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Name">The name of the slice.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record CutOverSlice(PrologueId Prologue, RebuildSliceName Name)
{
    /// <summary>
    /// Handles the command, refusing a slice that is not shadowing, one that was never compared and one with unresolved differences.
    /// </summary>
    /// <param name="board">The board, read under the command's consistency boundary, so a difference found while the command runs cannot be missed.</param>
    /// <returns>The cut over, or a refusal.</returns>
    public Result<SliceCutOver, ValidationResult> Handle(RebuildBoard? board) =>
        RebuildRules.RefuseCutOver(board, Name) is { } refusal
            ? refusal
            : new SliceCutOver(RebuildRules.Find(board, Name)!.Name);
}

/// <summary>
/// A slice was cut over.
/// </summary>
/// <param name="Name">The name of the slice.</param>
[EventType]
public record SliceCutOver(RebuildSliceName Name);
