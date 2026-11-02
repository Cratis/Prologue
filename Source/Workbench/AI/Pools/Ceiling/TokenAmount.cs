// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Pools.Ceiling;

/// <summary>
/// A number of tokens: what a pool may use in a day, or what it did use.
/// </summary>
/// <param name="Value">The number of tokens.</param>
public record TokenAmount(long Value) : ConceptAs<long>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="long"/> to a <see cref="TokenAmount"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator TokenAmount(long value) => new(value);
}
