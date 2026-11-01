// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Rebuild.Drift;

/// <summary>
/// A column a command was seen writing, as "schema.table.column". A change that names no column is written as "schema.table.*".
/// </summary>
/// <param name="Value">The underlying value.</param>
public record WrittenColumn(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly WrittenColumn None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="WrittenColumn"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator WrittenColumn(string value) => new(value);
}
