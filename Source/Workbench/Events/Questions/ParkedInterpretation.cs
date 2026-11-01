// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Workbench.Events.Interpreting;

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// An interpretation that waits for the interpreter's questions to be answered. The state holds the whole conversation with the language model,
/// so the interpretation resumes from it.
/// </summary>
/// <param name="State">The state of the interpreter session.</param>
/// <param name="Context">What the interpretation needs to finish.</param>
public record ParkedInterpretation(InterpreterSessionState State, InterpretationContext Context);
