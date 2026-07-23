// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents the user's answer to a pending <see cref="InterpreterQuestion"/> — either the label of one of the
/// question's choices or free text, since the user always has an implicit "other" option.
/// </summary>
/// <param name="QuestionId">The identifier of the question being answered.</param>
/// <param name="Value">The chosen label or the free-text answer.</param>
public record InterpreterAnswer(Guid QuestionId, string Value);
