// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The path inside a repository where one system's code lives; empty when the system is the whole repository.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SubPath(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SubPath"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SubPath(string value) => new(value);
}
