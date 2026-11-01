// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues.Output;

/// <summary>
/// Chooses where the Extractor sends its captures.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Kind">The kind of output.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record ChooseOutput(PrologueId Prologue, CaptureOutput Kind)
{
    /// <summary>
    /// Handles the command, refusing an output that is already chosen.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The output chosen, or a refusal.</returns>
    public Result<OutputChosen, ValidationResult> Handle(PrologueDefinition? prologue) => prologue switch
    {
        null => ValidationResult.Error("The prologue is not defined"),
        _ when prologue.Output == Kind => ValidationResult.Error("This output is already chosen"),
        _ => new OutputChosen(Kind)
    };
}

/// <summary>
/// The output for captures was chosen.
/// </summary>
/// <param name="Kind">The kind of output.</param>
[EventType]
public record OutputChosen(CaptureOutput Kind);
