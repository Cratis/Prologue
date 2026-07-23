// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents a question together with the answer the user gave — kept in the session state so the conversation
/// can be replayed to the stateless language model and audited afterwards.
/// </summary>
/// <param name="Question">The question that was asked.</param>
/// <param name="Answer">The answer the user gave — a choice label or free text.</param>
public record AnsweredQuestion(InterpreterQuestion Question, string Answer);
