// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Sharing;

/// <summary>
/// Whether the map of a prologue is shared, and the token in its read-only link.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Token">The token in the link; none when the map is not shared.</param>
[ReadModel]
public record SharedMap(PrologueId Id, ShareToken Token)
{
    /// <summary>
    /// Gets whether the map is shared.
    /// </summary>
    public bool IsShared => Token != ShareToken.None;

    /// <summary>
    /// Observes whether the map of a prologue is shared.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="readModels">Observes the stored shares.</param>
    /// <returns>The share, which has no token while the map is not shared.</returns>
    public static ISubject<IEnumerable<SharedMap>> ObserveSharedMap(PrologueId prologue, IReadModels readModels) =>
        readModels.Observe<SharedMap>(share => share.Id == prologue);

    /// <summary>
    /// Observes the share a token opens, which is how the read-only link finds the prologue it shows.
    /// </summary>
    /// <param name="token">The token in the link.</param>
    /// <param name="readModels">Observes the stored shares.</param>
    /// <returns>The share; nothing when no map is shared with that token.</returns>
    public static ISubject<IEnumerable<SharedMap>> ObserveSharedMapByToken(ShareToken token, IReadModels readModels) =>
        readModels.Observe<SharedMap>(share => share.Token == token);

    /// <summary>
    /// Reduces a map being shared.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue.</param>
    /// <returns>The new state.</returns>
    public static SharedMap Reduce(MapShared @event, SharedMap? current, EventContext context) =>
        new(new PrologueId(Guid.Parse(context.EventSourceId.Value)), @event.Token);

    /// <summary>
    /// Reduces sharing being stopped.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue.</param>
    /// <returns>The new state.</returns>
    public static SharedMap Reduce(MapSharingStopped @event, SharedMap? current, EventContext context) =>
        new(new PrologueId(Guid.Parse(context.EventSourceId.Value)), ShareToken.None);
}
