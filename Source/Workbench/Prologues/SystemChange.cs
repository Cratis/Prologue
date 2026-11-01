// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Prologues;

/// <summary>
/// Applies a change to one system of a prologue while a read model is reduced.
/// </summary>
static class SystemChange
{
    /// <summary>
    /// Changes one system.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="id">The system to change.</param>
    /// <param name="change">The change.</param>
    /// <returns>The new state; null when there is no current state.</returns>
    internal static PrologueDefinition? Apply(PrologueDefinition? current, SystemId id, Func<PrologueSystem, PrologueSystem> change) =>
        current is null
            ? null
            : current with { Systems = [.. current.Systems.Select(system => system.Id == id ? change(system) : system)] };
}
