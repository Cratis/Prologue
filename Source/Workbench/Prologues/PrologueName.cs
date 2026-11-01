// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues;

/// <summary>
/// The name of a prologue.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record PrologueName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="PrologueName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator PrologueName(string value) => new(value);
}
