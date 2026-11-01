// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The branch of a repository.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record BranchName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="BranchName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator BranchName(string value) => new(value);
}
