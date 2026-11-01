// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The identity of a system inside a prologue.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SystemId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Gets the identity representing no system.
    /// </summary>
    public static readonly SystemId NotSet = new(Guid.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="SystemId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SystemId(Guid value) => new(value);

    /// <summary>
    /// Creates a new identity.
    /// </summary>
    /// <returns>A new <see cref="SystemId"/>.</returns>
    public static SystemId New() => new(Guid.NewGuid());
}
