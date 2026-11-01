// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// A line in a source file.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record LineNumber(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly LineNumber None = new(0);

    /// <summary>
    /// Implicitly converts a <see cref="int"/> to a <see cref="LineNumber"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator LineNumber(int value) => new(value);
}
