// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events;

/// <summary>
/// The name of a slice in the interpreted model.
/// </summary>
/// <param name="Value">The value.</param>
public record SliceName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SliceName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SliceName(string value) => new(value);
}
