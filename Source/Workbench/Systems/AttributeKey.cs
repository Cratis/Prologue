// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The key of a span attribute whose value is allowed to be captured.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AttributeKey(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="AttributeKey"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator AttributeKey(string value) => new(value);
}
