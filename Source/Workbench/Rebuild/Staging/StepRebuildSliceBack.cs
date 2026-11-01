// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Staging;

/// <summary>
/// Steps a slice back one stage, for example to send a route back to the legacy system after a cut over went wrong.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Name">The name of the slice.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record StepRebuildSliceBack(PrologueId Prologue, RebuildSliceName Name)
{
    /// <summary>
    /// Handles the command, refusing an unknown slice and one that is still only observed.
    /// </summary>
    /// <param name="board">The board, read under the command's consistency boundary.</param>
    /// <returns>The step back, or a refusal.</returns>
    public Result<RebuildSliceSteppedBack, ValidationResult> Handle(RebuildBoard? board) =>
        RebuildRules.Find(board, Name) switch
        {
            null => ValidationResult.Error($"There is no slice called \"{Name.Value.Trim()}\""),
            { Stage: RebuildStage.Observed } slice => ValidationResult.Error($"{slice.Name.Value} is only observed; there is nothing to step back from"),
            { } slice => new RebuildSliceSteppedBack(slice.Name, slice.Stage - 1)
        };
}

/// <summary>
/// A slice stepped back one stage.
/// </summary>
/// <param name="Name">The name of the slice.</param>
/// <param name="To">The stage it is at now.</param>
[EventType]
public record RebuildSliceSteppedBack(RebuildSliceName Name, RebuildStage To);
