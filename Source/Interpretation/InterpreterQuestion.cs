// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents a question the language model asked because it is genuinely uncertain about a decision that
/// materially changes the model. Choices may be empty for a pure free-text question, and UIs always add an
/// implicit free-text "other" option on top of the choices.
/// </summary>
/// <param name="Id">The unique identifier of the question — the identifier an <see cref="InterpreterAnswer"/> refers to.</param>
/// <param name="Prompt">The question itself, phrased for the user.</param>
/// <param name="Choices">The choices offered; empty for a pure free-text question.</param>
/// <param name="Context">Why the model is uncertain — the background that helps the user answer.</param>
public record InterpreterQuestion(Guid Id, string Prompt, IReadOnlyList<QuestionChoice> Choices, string Context);
