// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Slicing;

/// <summary>
/// Stops tracking a slice. A slice that has been cut over is serving a route and cannot be removed; step it back first.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Name">The name of the slice.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record RemoveRebuildSlice(PrologueId Prologue, RebuildSliceName Name)
{
    /// <summary>
    /// Handles the command, refusing an unknown slice and one that is cut over.
    /// </summary>
    /// <param name="board">The board, read under the command's consistency boundary.</param>
    /// <returns>The slice removed, or a refusal.</returns>
    public Result<RebuildSliceRemoved, ValidationResult> Handle(RebuildBoard? board) =>
        RebuildRules.Find(board, Name) switch
        {
            null => ValidationResult.Error($"There is no slice called \"{Name.Value.Trim()}\""),
            { Stage: RebuildStage.CutOver } slice => ValidationResult.Error($"{slice.Name.Value} serves its route now. Step it back before removing it"),
            { } slice => new RebuildSliceRemoved(slice.Name)
        };
}

/// <summary>
/// A slice stopped being tracked.
/// </summary>
/// <param name="Name">The name of the slice.</param>
[EventType]
public record RebuildSliceRemoved(RebuildSliceName Name);
