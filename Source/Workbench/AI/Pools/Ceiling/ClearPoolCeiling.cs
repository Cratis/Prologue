// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Providers.Pools;
using Cratis.Arc.Chronicle.ReadModels;

namespace Cratis.Prologue.Workbench.AI.Pools.Ceiling;

/// <summary>
/// Command for taking the daily ceiling off a provider pool.
/// </summary>
/// <param name="Pool">The <see cref="AIProviderPoolId"/>.</param>
[Command]
public record ClearPoolCeiling(AIProviderPoolId Pool) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Pool.Value.ToString();

    /// <summary>
    /// Handles the command. Clearing a pool that has no ceiling records nothing.
    /// </summary>
    /// <param name="current">The pool's ceiling as it stands, if it has one.</param>
    /// <returns>The event, or nothing when the pool has no ceiling.</returns>
    public IEnumerable<PoolCeilingCleared> Handle([Unprotected("Clearing twice at once is harmless; the guard only avoids repeating the same fact")] PoolCeiling? current) =>
        current is null ? [] : [new PoolCeilingCleared()];
}

/// <summary>
/// Event raised when the daily ceiling of a provider pool was cleared.
/// </summary>
[EventType]
public record PoolCeilingCleared;
