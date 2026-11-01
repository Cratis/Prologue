// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery;

/// <summary>
/// A one-based line number in a source file.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record LineNumber(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="int"/> to a <see cref="LineNumber"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator LineNumber(int value) => new(value);
}
