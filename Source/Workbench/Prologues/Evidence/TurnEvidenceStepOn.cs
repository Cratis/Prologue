// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues.Evidence;

/// <summary>
/// Turns an evidence step on.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Step">The step to turn on.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record TurnEvidenceStepOn(PrologueId Prologue, EvidenceStep Step)
{
    /// <summary>
    /// Handles the command, refusing a step that is already on.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The step turned on, or a refusal.</returns>
    public Result<EvidenceStepTurnedOn, ValidationResult> Handle(PrologueDefinition? prologue) =>
        EvidenceRules.RefuseTurningOn(prologue, Step) is { } refusal ? refusal : new EvidenceStepTurnedOn(Step);
}

/// <summary>
/// An evidence step was turned on.
/// </summary>
/// <param name="Step">The step.</param>
[EventType]
public record EvidenceStepTurnedOn(EvidenceStep Step);
