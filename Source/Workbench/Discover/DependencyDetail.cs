// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover;

/// <summary>
/// How a dependency is used, such as the route called or the tables written.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record DependencyDetail(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="DependencyDetail"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator DependencyDetail(string value) => new(value);
}
