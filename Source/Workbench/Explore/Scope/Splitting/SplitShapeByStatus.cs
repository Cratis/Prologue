// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Scope.Splitting;

/// <summary>
/// Interprets a command shape once per response status instead of once as a whole.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Shape">The command shape, for example POST /api/loans/{id}/return.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record SplitShapeByStatus(PrologueId Prologue, CommandShape Shape)
{
    /// <summary>
    /// Handles the command. Asking for what already is the case records nothing.
    /// </summary>
    /// <param name="scope">The scope, read under the command's consistency boundary.</param>
    /// <returns>The event, nothing, or a refusal.</returns>
    public Result<IEnumerable<ShapeSplitByStatus>, ValidationResult> Handle(InterpretationScope? scope) =>
        ShapeScopeRules.Decide(Shape, InterpretationScopes.SplitsByStatus(scope, Shape), shape => new ShapeSplitByStatus(shape));
}

/// <summary>
/// A command shape is interpreted once per response status.
/// </summary>
/// <param name="Shape">The command shape.</param>
[EventType]
public record ShapeSplitByStatus(CommandShape Shape);
