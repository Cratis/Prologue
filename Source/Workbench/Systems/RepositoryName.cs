// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The repository a system's code lives in, such as owner/repository, or the folder for a local system.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record RepositoryName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="RepositoryName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator RepositoryName(string value) => new(value);
}
