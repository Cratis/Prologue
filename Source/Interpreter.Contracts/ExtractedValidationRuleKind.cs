// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents the kind of validation an <see cref="ExtractedValidationRule"/> expresses.
/// </summary>
public enum ExtractedValidationRuleKind
{
    /// <summary>
    /// The property must have a value — derived from a non-nullable column.
    /// </summary>
    Required,

    /// <summary>
    /// The property's value must not exceed a maximum length — derived from a column's declared maximum length.
    /// </summary>
    MaxLength,

    /// <summary>
    /// The property's value must have a minimum length.
    /// </summary>
    MinLength,

    /// <summary>
    /// The property's value must match a pattern.
    /// </summary>
    Pattern
}
