// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Rebuild.Differences;
using Cratis.Prologue.Workbench.Rebuild.Parity;
using Cratis.Prologue.Workbench.Rebuild.Slicing;
using Cratis.Prologue.Workbench.Rebuild.Staging;

namespace Cratis.Prologue.Workbench.Rebuild;

/// <summary>
/// The slices a prologue is rebuilding, how far each has come and how its decisions compare with the legacy system's. Parity is
/// counted as each observation arrives, never when the page opens. The board follows only events about slices and parity, never captures.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Slices">The slices, in the order they were added.</param>
[ReadModel]
public record RebuildBoard(PrologueId Id, IEnumerable<RebuildSlice> Slices)
{
    /// <summary>
    /// Observes the board of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored boards.</param>
    /// <returns>The board, pushed again whenever a slice or its parity changes.</returns>
    public static ISubject<RebuildBoard> ObserveRebuildBoard(PrologueId id, IReadModels readModels) =>
        readModels.Observe<RebuildBoard>(board => board.Id == id).FirstItem();

    /// <summary>
    /// Reduces a slice being added, observed.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildBoard Reduce(RebuildSliceAdded @event, RebuildBoard? current, EventContext context) =>
        RebuildBoards.Existing(current, context).WithSlice(new(@event.Name, @event.Shapes, RebuildStage.Observed, 0, 0, 0, 0, []));

    /// <summary>
    /// Reduces a slice being removed.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildBoard Reduce(RebuildSliceRemoved @event, RebuildBoard? current, EventContext context)
    {
        var board = RebuildBoards.Existing(current, context);
        return board with { Slices = board.Slices.Where(slice => slice.Name != @event.Name) };
    }

    /// <summary>
    /// Reduces a slice being marked modeled.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildBoard Reduce(RebuildSliceMarkedModeled @event, RebuildBoard? current, EventContext context) =>
        RebuildBoards.Existing(current, context).Staged(@event.Name, RebuildStage.Modeled);

    /// <summary>
    /// Reduces a slice being marked built.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildBoard Reduce(RebuildSliceMarkedBuilt @event, RebuildBoard? current, EventContext context) =>
        RebuildBoards.Existing(current, context).Staged(@event.Name, RebuildStage.Built);

    /// <summary>
    /// Reduces shadowing of a slice starting.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildBoard Reduce(ShadowingStarted @event, RebuildBoard? current, EventContext context) =>
        RebuildBoards.Existing(current, context).Staged(@event.Name, RebuildStage.Shadowing);

    /// <summary>
    /// Reduces a slice being cut over.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildBoard Reduce(SliceCutOver @event, RebuildBoard? current, EventContext context) =>
        RebuildBoards.Existing(current, context).Staged(@event.Name, RebuildStage.CutOver);

    /// <summary>
    /// Reduces a slice stepping back.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildBoard Reduce(RebuildSliceSteppedBack @event, RebuildBoard? current, EventContext context) =>
        RebuildBoards.Existing(current, context).Staged(@event.Name, @event.To);

    /// <summary>
    /// Reduces an observation: it counts for the slice that replaces its command shape, and a mismatch is an open difference until resolved.
    /// An observation of a shape no slice replaces counts for none.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildBoard Reduce(ParityObserved @event, RebuildBoard? current, EventContext context) =>
        RebuildBoards.Existing(current, context).Observed(@event);

    /// <summary>
    /// Reduces a difference being accepted: it is no longer open.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildBoard Reduce(DifferenceAccepted @event, RebuildBoard? current, EventContext context) =>
        RebuildBoards.Existing(current, context).Resolved(@event.Observation);

    /// <summary>
    /// Reduces a difference being marked to be matched to the legacy behavior: it is no longer open.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildBoard Reduce(DifferenceToBeMatched @event, RebuildBoard? current, EventContext context) =>
        RebuildBoards.Existing(current, context).Resolved(@event.Observation);

    RebuildBoard WithSlice(RebuildSlice slice) => this with { Slices = [.. Slices, slice] };
}
