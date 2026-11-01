// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// What recording an answer in a waiting interpretation did.
/// </summary>
/// <param name="Question">The question that was answered; null when no such question was waiting.</param>
/// <param name="Complete">Whether no question is left, so the interpretation can carry on.</param>
public record AnswerOutcome(InterpreterQuestion? Question, bool Complete);
