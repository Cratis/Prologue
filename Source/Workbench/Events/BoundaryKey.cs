// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events;

/// <summary>
/// Identifies a consistency boundary derived from evidence: the command shape and the kind of boundary, so a choice survives the evidence being derived again.
/// </summary>
/// <param name="Value">The value.</param>
public record BoundaryKey(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="BoundaryKey"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator BoundaryKey(string value) => new(value);
}
