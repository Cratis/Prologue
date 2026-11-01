// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// Identifies one question the interpreter asked in one interpretation. The same question asked in another interpretation has another id and the same <see cref="QuestionKey"/>.
/// </summary>
/// <param name="Value">The value.</param>
public record QuestionId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="QuestionId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator QuestionId(Guid value) => new(value);
}
