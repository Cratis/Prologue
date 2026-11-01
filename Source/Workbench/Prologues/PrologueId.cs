// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Prologues;

/// <summary>
/// The identity of a prologue; every fact about the prologue and its systems is appended to its stream.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record PrologueId(Guid Value) : EventSourceId<Guid>(Value)
{
    /// <summary>
    /// Gets the identity representing no prologue.
    /// </summary>
    public static readonly PrologueId NotSet = new(Guid.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="PrologueId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator PrologueId(Guid value) => new(value);

    /// <summary>
    /// Creates a new identity.
    /// </summary>
    /// <returns>A new <see cref="PrologueId"/>.</returns>
    public static PrologueId New() => new(Guid.NewGuid());
}
