// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Rebuild;

/// <summary>
/// The name of a slice to rebuild, such as LendBook. It is unique within a prologue.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record RebuildSliceName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly RebuildSliceName None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="RebuildSliceName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator RebuildSliceName(string value) => new(value);
}
