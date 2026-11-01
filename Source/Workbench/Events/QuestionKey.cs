// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events;

/// <summary>
/// Identifies a question of the interpreter across interpretations: the normalized prompt, so an answer given once is still the answer the next time the language model asks the same thing.
/// </summary>
/// <param name="Value">The value.</param>
public record QuestionKey(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="QuestionKey"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator QuestionKey(string value) => new(value);
}
