// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// The exception that is thrown when an answer refers to a question that is not pending in the interpreter
/// session — it was either never asked or has already been answered.
/// </summary>
/// <param name="questionId">The identifier of the question the answer referred to.</param>
public class QuestionNotPending(Guid questionId)
    : Exception($"Question '{questionId}' is not pending in the interpreter session — it was either never asked or has already been answered.");
