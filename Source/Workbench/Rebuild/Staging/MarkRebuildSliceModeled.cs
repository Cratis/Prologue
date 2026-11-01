// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Staging;

/// <summary>
/// Marks a slice as part of the event model.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Name">The name of the slice.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record MarkRebuildSliceModeled(PrologueId Prologue, RebuildSliceName Name)
{
    /// <summary>
    /// Handles the command, refusing a slice that is not observed.
    /// </summary>
    /// <param name="board">The board, read under the command's consistency boundary.</param>
    /// <returns>The slice marked, or a refusal.</returns>
    public Result<RebuildSliceMarkedModeled, ValidationResult> Handle(RebuildBoard? board) =>
        RebuildRules.RefuseMoveFrom(board, Name, RebuildStage.Observed) is { } refusal
            ? refusal
            : new RebuildSliceMarkedModeled(RebuildRules.Find(board, Name)!.Name);
}

/// <summary>
/// A slice was marked as modeled.
/// </summary>
/// <param name="Name">The name of the slice.</param>
[EventType]
public record RebuildSliceMarkedModeled(RebuildSliceName Name);
