// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// A request to interpret a prologue's captures, or to carry on once the interpreter's questions are answered.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Resume">Whether to continue the interpretation that waits for answers instead of starting a new one.</param>
public record InterpretationJob(PrologueId Prologue, bool Resume);
