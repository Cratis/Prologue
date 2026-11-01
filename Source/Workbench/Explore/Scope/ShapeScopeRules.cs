// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Scope;

/// <summary>
/// The rules the commands that change the interpretation scope share.
/// </summary>
public static class ShapeScopeRules
{
    /// <summary>
    /// Gets why a shape cannot be put in or out of scope, when it cannot.
    /// </summary>
    /// <param name="shape">The command shape.</param>
    /// <returns>The refusal, or null when the shape can be used.</returns>
    public static ValidationResult? RefuseBlank(CommandShape shape) =>
        shape.IsNone ? ValidationResult.Error("Pick a command shape first") : null;

    /// <summary>
    /// Decides a change of scope: refuse a blank shape, record nothing when the scope already is the way it is asked to be, and
    /// otherwise record the change.
    /// </summary>
    /// <typeparam name="TEvent">The event that records the change.</typeparam>
    /// <param name="shape">The command shape.</param>
    /// <param name="alreadyTrue">Whether the scope already is the way it is asked to be.</param>
    /// <param name="record">Creates the event.</param>
    /// <returns>The event, nothing, or a refusal.</returns>
    public static Result<IEnumerable<TEvent>, ValidationResult> Decide<TEvent>(CommandShape shape, bool alreadyTrue, Func<CommandShape, TEvent> record)
    {
        if (RefuseBlank(shape) is { } refusal)
        {
            return refusal;
        }

        IEnumerable<TEvent> events = alreadyTrue ? [] : [record(shape)];
        return Result<IEnumerable<TEvent>, ValidationResult>.Success(events);
    }
}
