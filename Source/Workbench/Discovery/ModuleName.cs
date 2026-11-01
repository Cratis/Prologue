// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery;

/// <summary>
/// The name of a module in a system: a group of related capabilities, such as a controller.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ModuleName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ModuleName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ModuleName(string value) => new(value);
}
