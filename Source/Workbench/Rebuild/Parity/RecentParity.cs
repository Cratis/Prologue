// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// The latest parity observations of a prologue, newest first. One history follows an entity through them.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Observations">The observations, at most <see cref="Limit"/>.</param>
[ReadModel]
public record RecentParity(PrologueId Id, IEnumerable<ParityObserved> Observations)
{
    /// <summary>
    /// The most observations kept per prologue.
    /// </summary>
    public const int Limit = 200;

    /// <summary>
    /// Observes the latest observations of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored observations.</param>
    /// <returns>The latest observations, pushed again whenever one arrives.</returns>
    public static ISubject<RecentParity> ObserveRecentParity(PrologueId id, IReadModels readModels) =>
        readModels.Observe<RecentParity>(recent => recent.Id == id).FirstItem();

    /// <summary>
    /// Reduces an observation arriving: it becomes the newest and the oldest beyond the limit falls off.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RecentParity Reduce(ParityObserved @event, RecentParity? current, EventContext context) =>
        new(current?.Id ?? RebuildRules.PrologueOf(context), [@event, .. (current?.Observations ?? []).Take(Limit - 1)]);
}
