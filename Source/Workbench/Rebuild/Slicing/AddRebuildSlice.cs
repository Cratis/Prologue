// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Slicing;

/// <summary>
/// Adds a slice to rebuild: a name and the legacy command shapes it replaces. Every slice starts as observed.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Name">The name of the slice, such as LendBook.</param>
/// <param name="Shapes">The legacy command shapes it replaces, such as POST /api/loans.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record AddRebuildSlice(PrologueId Prologue, RebuildSliceName Name, IEnumerable<CommandShape> Shapes)
{
    /// <summary>
    /// Handles the command, refusing a blank or taken name, no shape, a shape that is not METHOD /path and a shape another slice replaces.
    /// </summary>
    /// <param name="board">The board, read under the command's consistency boundary.</param>
    /// <returns>The slice added, or a refusal.</returns>
    public Result<RebuildSliceAdded, ValidationResult> Handle(RebuildBoard? board) =>
        RebuildRules.RefuseAdd(board, Name, Shapes) is { } refusal
            ? refusal
            : new RebuildSliceAdded(new(Name.Value.Trim()), [.. Shapes.Select(shape => RebuildRules.NormalizeShape(shape)!).Distinct()]);
}

/// <summary>
/// A slice to rebuild was added.
/// </summary>
/// <param name="Name">The name of the slice.</param>
/// <param name="Shapes">The legacy command shapes it replaces.</param>
[EventType]
public record RebuildSliceAdded(RebuildSliceName Name, IEnumerable<CommandShape> Shapes);
