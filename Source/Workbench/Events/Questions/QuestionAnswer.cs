// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// The answer a person gave to a question of the interpreter.
/// </summary>
/// <param name="Key">The question, by its normalized prompt.</param>
/// <param name="Prompt">The question as it was asked.</param>
/// <param name="Answer">The label of the choice picked or the free text written.</param>
public record QuestionAnswer(QuestionKey Key, string Prompt, string Answer);
