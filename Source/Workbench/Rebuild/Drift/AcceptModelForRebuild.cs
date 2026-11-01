// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Drift;

/// <summary>
/// Marks the event model of the prologue as accepted for the rebuild. What the legacy system did until now is what the model covers;
/// from now on a new command shape, or a column a known shape newly writes, is drift.
/// </summary>
/// <remarks>
/// This is the seam the Model step feeds: when it accepts a model it executes this command, or appends <see cref="ModelAcceptedForRebuild"/>
/// to the prologue's stream. Nothing else in Rebuild needs to change.
/// </remarks>
/// <param name="Prologue">The prologue.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record AcceptModelForRebuild(PrologueId Prologue)
{
    /// <summary>
    /// Handles the command, refusing a model that is accepted already.
    /// </summary>
    /// <param name="drift">The drift, read under the command's consistency boundary.</param>
    /// <returns>The acceptance, or a refusal.</returns>
    public Result<ModelAcceptedForRebuild, ValidationResult> Handle(ModelDrift? drift) =>
        drift?.Accepted == true
            ? ValidationResult.Error("The model is accepted already")
            : new ModelAcceptedForRebuild();
}

/// <summary>
/// The event model of the prologue was accepted for the rebuild. Captures that arrive afterwards are checked against what was seen before.
/// </summary>
[EventType]
public record ModelAcceptedForRebuild;
