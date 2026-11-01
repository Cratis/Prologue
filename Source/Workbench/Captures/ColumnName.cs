// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// The name of a column a captured transaction changed.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ColumnName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly ColumnName None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ColumnName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ColumnName(string value) => new(value);
}
