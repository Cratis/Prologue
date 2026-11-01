// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues.Evidence;

/// <summary>
/// The rules for which evidence steps a prologue gathers.
/// </summary>
public static class EvidenceRules
{
    /// <summary>
    /// Gets why a step cannot be turned off, when it cannot.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="step">The step to turn off.</param>
    /// <returns>The refusal, or null when the step can be turned off.</returns>
    public static ValidationResult? RefuseTurningOff(PrologueDefinition? prologue, EvidenceStep step)
    {
        if (prologue is null)
        {
            return ValidationResult.Error("The prologue is not defined");
        }

        if (!prologue.Steps.IsOn(step))
        {
            return ValidationResult.Error($"{step} is already off");
        }

        var remaining = prologue.Steps.With(step, false);
        return !remaining.Discover && !remaining.Capture
            ? ValidationResult.Error("Keep Discover or Capture on. Prologue needs some evidence to work from.")
            : null;
    }

    /// <summary>
    /// Gets why a step cannot be turned on, when it cannot.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="step">The step to turn on.</param>
    /// <returns>The refusal, or null when the step can be turned on.</returns>
    public static ValidationResult? RefuseTurningOn(PrologueDefinition? prologue, EvidenceStep step) => prologue switch
    {
        null => ValidationResult.Error("The prologue is not defined"),
        _ when prologue.Steps.IsOn(step) => ValidationResult.Error($"{step} is already on"),
        _ => null
    };
}
