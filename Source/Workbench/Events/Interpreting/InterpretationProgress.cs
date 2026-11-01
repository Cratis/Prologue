// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// How the interpretation of a prologue is going. Progress is operational state and lives in memory, never in the event log: looking is not a fact.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="State">Where the interpretation is.</param>
/// <param name="Message">What is happening, or what happened, in words.</param>
/// <param name="Refined">Whether the language model refined the result; false for heuristics only.</param>
/// <param name="Suggestions">How many events the last finished interpretation suggested.</param>
public record InterpretationProgress(PrologueId Prologue, InterpretationState State, string Message, bool Refined, int Suggestions)
{
    /// <summary>
    /// Gets the progress of a prologue nothing was run for.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The progress.</returns>
    public static InterpretationProgress Idle(PrologueId prologue) => new(prologue, InterpretationState.Idle, string.Empty, false, 0);
}
