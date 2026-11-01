// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Explore.Scope.Excluding;
using Cratis.Prologue.Workbench.Explore.Scope.Splitting;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Scope;

/// <summary>
/// What of a prologue's captures the interpretation step should look at. The interpretation step reads this read model (by the
/// prologue's id, with <see cref="Chronicle.ReadModels.IReadModels"/>; no document means everything is in scope): it leaves out every
/// shape in <see cref="Excluded"/>, and treats every shape in <see cref="Split"/> as one candidate per response status instead of one
/// candidate for the whole shape. Use <see cref="InterpretationScopes.Includes"/> and <see cref="InterpretationScopes.SplitsByStatus"/> rather than reading the lists.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Excluded">The command shapes left out of interpretation, for example a load generator.</param>
/// <param name="Split">The command shapes interpreted once per response status.</param>
[ReadModel]
public record InterpretationScope(PrologueId Id, IEnumerable<CommandShape> Excluded, IEnumerable<CommandShape> Split)
{
    /// <summary>
    /// Observes the scope of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored scope.</param>
    /// <returns>The scope, pushed again whenever it changes.</returns>
    public static ISubject<InterpretationScope> ObserveInterpretationScope(PrologueId id, IReadModels readModels) =>
        readModels.Observe<InterpretationScope>(scope => scope.Id == id).FirstItem();

    /// <summary>
    /// Reduces a shape being left out of interpretation.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static InterpretationScope Reduce(ShapeExcludedFromInterpretation @event, InterpretationScope? current, EventContext context)
    {
        var scope = InterpretationScopes.Existing(current, context);
        return scope with { Excluded = [.. scope.Excluded.Where(shape => shape != @event.Shape), @event.Shape] };
    }

    /// <summary>
    /// Reduces a shape being brought back into interpretation.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static InterpretationScope Reduce(ShapeIncludedInInterpretation @event, InterpretationScope? current, EventContext context)
    {
        var scope = InterpretationScopes.Existing(current, context);
        return scope with { Excluded = [.. scope.Excluded.Where(shape => shape != @event.Shape)] };
    }

    /// <summary>
    /// Reduces a shape being split by response status.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static InterpretationScope Reduce(ShapeSplitByStatus @event, InterpretationScope? current, EventContext context)
    {
        var scope = InterpretationScopes.Existing(current, context);
        return scope with { Split = [.. scope.Split.Where(shape => shape != @event.Shape), @event.Shape] };
    }

    /// <summary>
    /// Reduces a split being undone.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static InterpretationScope Reduce(ShapeSplitUndone @event, InterpretationScope? current, EventContext context)
    {
        var scope = InterpretationScopes.Existing(current, context);
        return scope with { Split = [.. scope.Split.Where(shape => shape != @event.Shape)] };
    }
}
