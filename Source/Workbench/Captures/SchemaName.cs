// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// The schema a captured table belongs to.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SchemaName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly SchemaName None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SchemaName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SchemaName(string value) => new(value);
}
