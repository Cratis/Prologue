// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Drift;

/// <summary>
/// What the legacy system does that the accepted model does not cover. Before the model is accepted it learns the command shapes and
/// the columns they write; after, a new shape or a newly written column becomes drift. It is kept up to date as captures arrive.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Accepted">Whether the model has been accepted for the rebuild.</param>
/// <param name="Known">What was seen before the model was accepted.</param>
/// <param name="Items">The drift, in the order it was first seen.</param>
[ReadModel]
public record ModelDrift(PrologueId Id, bool Accepted, IEnumerable<KnownShape> Known, IEnumerable<DriftItem> Items)
{
    /// <summary>
    /// Observes the drift of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored drift.</param>
    /// <returns>The drift, pushed again whenever it changes.</returns>
    public static ISubject<ModelDrift> ObserveModelDrift(PrologueId id, IReadModels readModels) =>
        readModels.Observe<ModelDrift>(drift => drift.Id == id).FirstItem();

    /// <summary>
    /// Reduces a capture arriving.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static ModelDrift Reduce(CaptureRecorded @event, ModelDrift? current, EventContext context) =>
        DriftDetection.Observe(RebuildBoards.Existing(current, context), @event);

    /// <summary>
    /// Reduces the model being accepted.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static ModelDrift Reduce(ModelAcceptedForRebuild @event, ModelDrift? current, EventContext context) =>
        RebuildBoards.Existing(current, context) with { Accepted = true };

    /// <summary>
    /// Reduces drift being acknowledged.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static ModelDrift Reduce(DriftAcknowledged @event, ModelDrift? current, EventContext context)
    {
        var drift = RebuildBoards.Existing(current, context);
        return drift with { Items = [.. drift.Items.Select(item => item.Key == @event.Key ? item with { Acknowledged = true } : item)] };
    }
}
