// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Journeys;

/// <summary>
/// An identity taken from a request path, such as 2649 in /api/loans/2649/return. It only tells captures of the same thing apart.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record EntityId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="EntityId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator EntityId(string value) => new(value);
}
