// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Scenarios;

/// <summary>
/// Helpers for reducing a <see cref="ScenarioScript"/>.
/// </summary>
internal static class ScenarioScripts
{
    /// <summary>
    /// Gets the current script, or an empty one for the prologue the event belongs to.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The script.</returns>
    internal static ScenarioScript Existing(ScenarioScript? current, EventContext context) =>
        current ?? new(new PrologueId(Guid.Parse(context.EventSourceId.Value)), [], ScenarioName.None);
}
