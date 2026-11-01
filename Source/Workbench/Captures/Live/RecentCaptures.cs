// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Captures.Live;

/// <summary>
/// The latest captures of a prologue, newest first.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Captures">The captures, at most <see cref="Limit"/>.</param>
[ReadModel]
public record RecentCaptures(PrologueId Id, IEnumerable<RecordedCapture> Captures)
{
    /// <summary>
    /// The most captures kept per prologue.
    /// </summary>
    public const int Limit = 200;

    /// <summary>
    /// Observes the latest captures of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored captures.</param>
    /// <returns>The latest captures, pushed again whenever one arrives.</returns>
    public static ISubject<RecentCaptures> ObserveRecentCaptures(PrologueId id, IReadModels readModels) =>
        readModels.Observe<RecentCaptures>(recent => recent.Id == id).FirstItem();

    /// <summary>
    /// Reduces a capture arriving: it becomes the newest and the oldest beyond the limit falls off.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RecentCaptures Reduce(CaptureRecorded @event, RecentCaptures? current, EventContext context) =>
        new(
            current?.Id ?? new PrologueId(Guid.Parse(context.EventSourceId.Value)),
            [RecordedCapture.From(@event), .. (current?.Captures ?? []).Take(Limit - 1)]);
}
