// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// A question the Workbench raises from its own evidence rather than the Interpreter's session, such as what writes to a table with no command are.
/// It stays open until a person answers it, and the answer is fed back into the next interpretation.
/// </summary>
/// <param name="Id">The identity people answer by, derived from the key so it is the same in every interpretation.</param>
/// <param name="Key">The key of the question; it names what the question is about, so the answer survives the numbers in the prompt changing.</param>
/// <param name="Prompt">The question.</param>
/// <param name="Context">What the evidence shows.</param>
/// <param name="Choices">The answers on offer.</param>
public record RaisedQuestion(QuestionId Id, QuestionKey Key, string Prompt, string Context, IEnumerable<QuestionOption> Choices);
