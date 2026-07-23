// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents a question as the language model phrased it in a <see cref="ModelRefinement"/> — before the session
/// assigns it an identity and turns it into an <see cref="InterpreterQuestion"/> for the user.
/// </summary>
/// <param name="Prompt">The question itself, phrased for the user.</param>
/// <param name="Context">Why the model is uncertain — the background that helps the user answer.</param>
/// <param name="Choices">The choices offered; empty for a pure free-text question.</param>
public record RefinementQuestion(string Prompt, string Context, IReadOnlyList<QuestionChoice> Choices);
