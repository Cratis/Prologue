// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Scope.Excluding;

/// <summary>
/// Brings a command shape that was left out back into interpretation.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Shape">The command shape, for example POST /api/loans/{id}/return.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record IncludeShapeInInterpretation(PrologueId Prologue, CommandShape Shape)
{
    /// <summary>
    /// Handles the command. Asking for what already is the case records nothing.
    /// </summary>
    /// <param name="scope">The scope, read under the command's consistency boundary.</param>
    /// <returns>The event, nothing, or a refusal.</returns>
    public Result<IEnumerable<ShapeIncludedInInterpretation>, ValidationResult> Handle(InterpretationScope? scope) =>
        ShapeScopeRules.Decide(Shape, InterpretationScopes.Includes(scope, Shape), shape => new ShapeIncludedInInterpretation(shape));
}

/// <summary>
/// A command shape that was left out is part of interpretation again.
/// </summary>
/// <param name="Shape">The command shape.</param>
[EventType]
public record ShapeIncludedInInterpretation(CommandShape Shape);
