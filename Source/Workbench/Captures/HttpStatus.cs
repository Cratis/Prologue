// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// The HTTP status code a command was answered with.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record HttpStatus(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly HttpStatus None = new(0);

    /// <summary>
    /// Implicitly converts a <see cref="int"/> to a <see cref="HttpStatus"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator HttpStatus(int value) => new(value);
}
