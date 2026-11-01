// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Scope;

/// <summary>
/// What the interpretation step asks of an <see cref="InterpretationScope"/>, and helpers for reducing it.
/// </summary>
public static class InterpretationScopes
{
    /// <summary>
    /// Gets whether the interpretation step should look at the captures of a shape.
    /// </summary>
    /// <param name="scope">The scope, or null when nothing was ever excluded or split.</param>
    /// <param name="shape">The command shape.</param>
    /// <returns>True unless the shape was excluded.</returns>
    public static bool Includes(InterpretationScope? scope, CommandShape shape) => scope?.Excluded.Contains(shape) != true;

    /// <summary>
    /// Gets whether the interpretation step should treat each response status of a shape as its own candidate.
    /// </summary>
    /// <param name="scope">The scope, or null when nothing was ever excluded or split.</param>
    /// <param name="shape">The command shape.</param>
    /// <returns>True when the shape was split by status.</returns>
    public static bool SplitsByStatus(InterpretationScope? scope, CommandShape shape) => scope?.Split.Contains(shape) == true;

    /// <summary>
    /// Gets the current scope, or an empty one for the prologue the event belongs to.
    /// </summary>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The scope.</returns>
    internal static InterpretationScope Existing(InterpretationScope? current, EventContext context) =>
        current ?? new(new PrologueId(Guid.Parse(context.EventSourceId.Value)), [], []);
}
