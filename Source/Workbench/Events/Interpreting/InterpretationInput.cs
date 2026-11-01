// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Questions;
using Cratis.Prologue.Workbench.Explore.Scope;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Everything an interpretation of a prologue starts from.
/// </summary>
/// <param name="Captures">The prologue's captures, as the capture store holds them.</param>
/// <param name="Scope">The interpretation scope, or null when nothing was ever excluded or split.</param>
/// <param name="Facts">What else is known about the prologue.</param>
/// <param name="Answers">The answers people already gave to the interpreter.</param>
public record InterpretationInput(IReadOnlyList<Capture> Captures, InterpretationScope? Scope, EvidenceFacts Facts, IReadOnlyList<QuestionAnswer> Answers);
