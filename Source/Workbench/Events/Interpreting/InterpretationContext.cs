// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Explore.Scope;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// What an interpretation needs to finish after it waited for the interpreter's questions to be answered. Plain data, so the interpretation
/// can carry on in a later run of the worker.
/// </summary>
/// <param name="Captures">The captures that are interpreted: the prologue's captures without the excluded shapes.</param>
/// <param name="Partitions">The parts of the captures that are interpreted on their own.</param>
/// <param name="Heuristic">The model the heuristics built.</param>
/// <param name="Facts">What else is known about the prologue.</param>
/// <param name="Scope">The interpretation scope.</param>
/// <param name="Answers">The answers people had already given when the interpretation started.</param>
public record InterpretationContext(
    IReadOnlyList<Capture> Captures,
    IReadOnlyList<CapturePartition> Partitions,
    ExtractionResult Heuristic,
    EvidenceFacts Facts,
    InterpretationScope? Scope,
    IReadOnlyList<QuestionAnswer> Answers);
