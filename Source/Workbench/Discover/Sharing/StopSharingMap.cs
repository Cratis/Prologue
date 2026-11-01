// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Sharing;

/// <summary>
/// Stops sharing the map of a prologue, which ends the read-only link.
/// </summary>
/// <param name="Prologue">The prologue.</param>
[Command]
public record StopSharingMap(PrologueId Prologue) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Prologue;

    /// <summary>
    /// Handles the command, refusing when the map is not shared.
    /// </summary>
    /// <param name="shared">Whether and how the map is shared.</param>
    /// <returns>The end of the share, or a refusal.</returns>
    public Result<MapSharingStopped, ValidationResult> Handle([Unprotected("Stopping twice at once records the same end twice at worst")] SharedMap? shared) =>
        shared is { IsShared: true }
            ? new MapSharingStopped(shared.Token)
            : ValidationResult.Error("The map is not shared");
}

/// <summary>
/// Sharing the map of a prologue was stopped, so its link no longer opens.
/// </summary>
/// <param name="Token">The token the link had.</param>
[EventType]
public record MapSharingStopped(ShareToken Token);
