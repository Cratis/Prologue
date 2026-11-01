// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories;

/// <summary>
/// The identifier of a GitHub App installation.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record InstallationId(long Value) : ConceptAs<long>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="long"/> to an <see cref="InstallationId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator InstallationId(long value) => new(value);
}
