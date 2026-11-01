// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// The commit a clone is at.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record CommitId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="CommitId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CommitId(string value) => new(value);
}
