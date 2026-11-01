// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Rebuild;

/// <summary>
/// The identity of one parity observation: one command mirrored to the new system and compared with the legacy answer.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ObservationId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="ObservationId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ObservationId(Guid value) => new(value);
}
