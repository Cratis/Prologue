// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// The number of files found.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record FileCount(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// Implicitly converts an <see cref="int"/> to a <see cref="FileCount"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator FileCount(int value) => new(value);
}
