// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues.Evidence;

/// <summary>
/// Turns an evidence step off. Discover or Capture always stays on.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Step">The step to turn off.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record TurnEvidenceStepOff(PrologueId Prologue, EvidenceStep Step)
{
    /// <summary>
    /// Handles the command, refusing to leave the prologue without Discover and Capture.
    /// </summary>
    /// <param name="prologue">The prologue, read under the command's consistency boundary.</param>
    /// <returns>The step turned off, or a refusal.</returns>
    public Result<EvidenceStepTurnedOff, ValidationResult> Handle(PrologueDefinition? prologue) =>
        EvidenceRules.RefuseTurningOff(prologue, Step) is { } refusal ? refusal : new EvidenceStepTurnedOff(Step);
}

/// <summary>
/// An evidence step was turned off.
/// </summary>
/// <param name="Step">The step.</param>
[EventType]
public record EvidenceStepTurnedOff(EvidenceStep Step);
