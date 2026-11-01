// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues.Correlation;

/// <summary>
/// Sets the window within which an HTTP command claims the database transactions and spans it caused.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Window">The window.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record SetCorrelationWindow(PrologueId Prologue, CorrelationWindow Window)
{
    /// <summary>
    /// Handles the command, refusing a window outside the supported range or one that is already set.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The window set, or a refusal.</returns>
    public Result<CorrelationWindowSet, ValidationResult> Handle(PrologueDefinition? prologue)
    {
        if (prologue is null)
        {
            return ValidationResult.Error("The prologue is not defined");
        }

        if (Window.Value is < CorrelationWindow.Minimum or > CorrelationWindow.Maximum)
        {
            return ValidationResult.Error($"Choose a window between {CorrelationWindow.Minimum} and {CorrelationWindow.Maximum} milliseconds");
        }

        return prologue.Correlation == Window ? ValidationResult.Error("The window is already set to this value") : new CorrelationWindowSet(Window);
    }
}

/// <summary>
/// The correlation window was set.
/// </summary>
/// <param name="Window">The window.</param>
[EventType]
public record CorrelationWindowSet(CorrelationWindow Window);
