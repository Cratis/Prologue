// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Rebuild.Differences;
using Cratis.Prologue.Workbench.Rebuild.Drift;
using Cratis.Prologue.Workbench.Rebuild.Parity;

namespace Cratis.Prologue.Workbench.Rebuild;

/// <summary>
/// The changes to a <see cref="RebuildBoard"/> its reducers make.
/// </summary>
internal static class RebuildBoards
{
    /// <summary>
    /// Gets the current board, or an empty one for the prologue the event belongs to.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The board.</returns>
    internal static RebuildBoard Existing(RebuildBoard? current, EventContext context) =>
        current ?? new(RebuildRules.PrologueOf(context), []);

    /// <summary>
    /// Gets the current differences, or an empty set for the prologue the event belongs to.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The differences.</returns>
    internal static RebuildDifferences Existing(RebuildDifferences? current, EventContext context) =>
        current ?? new(RebuildRules.PrologueOf(context), []);

    /// <summary>
    /// Gets the current drift, or an empty one for the prologue the event belongs to.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The drift.</returns>
    internal static ModelDrift Existing(ModelDrift? current, EventContext context) =>
        current ?? new(RebuildRules.PrologueOf(context), false, [], []);

    /// <summary>
    /// Moves a slice to a stage.
    /// </summary>
    /// <param name="board">The board.</param>
    /// <param name="name">The slice.</param>
    /// <param name="stage">The stage it is at now.</param>
    /// <returns>The new board.</returns>
    internal static RebuildBoard Staged(this RebuildBoard board, RebuildSliceName name, RebuildStage stage) =>
        board with { Slices = [.. board.Slices.Select(slice => slice.Name == name ? slice with { Stage = stage } : slice)] };

    /// <summary>
    /// Counts an observation for the slice replacing its command shape.
    /// </summary>
    /// <param name="board">The board.</param>
    /// <param name="observed">The observation.</param>
    /// <returns>The new board.</returns>
    internal static RebuildBoard Observed(this RebuildBoard board, ParityObserved observed)
    {
        var owner = board.Slices.FirstOrDefault(slice => slice.Replaces(observed.Shape));
        return owner is null
            ? board
            : board with { Slices = [.. board.Slices.Select(slice => slice.Name == owner.Name ? Counted(slice, observed) : slice)] };
    }

    /// <summary>
    /// Takes a difference off the open ones of whichever slice holds it.
    /// </summary>
    /// <param name="board">The board.</param>
    /// <param name="observation">The observation that was resolved.</param>
    /// <returns>The new board.</returns>
    internal static RebuildBoard Resolved(this RebuildBoard board, ObservationId observation) =>
        board with { Slices = [.. board.Slices.Select(slice => slice with { OpenDifferences = slice.OpenDifferences.Where(open => open != observation) })] };

    static RebuildSlice Counted(RebuildSlice slice, ParityObserved observed) => observed.Outcome switch
    {
        ParityOutcome.Matched => slice with { Matched = slice.Matched + 1 },
        ParityOutcome.Mismatched => slice with { Mismatched = slice.Mismatched + 1, OpenDifferences = [.. slice.OpenDifferences, observed.Observation] },
        ParityOutcome.Unavailable => slice with { Unavailable = slice.Unavailable + 1 },
        _ => slice with { Skipped = slice.Skipped + 1 }
    };
}
