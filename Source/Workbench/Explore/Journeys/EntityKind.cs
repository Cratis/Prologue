// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Journeys;

/// <summary>
/// The kind of thing an identity in a request path identifies, taken from the path segment in front of it: "loans" in /api/loans/2649/return.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record EntityKind(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the kind of an identity that has no segment in front of it.
    /// </summary>
    public static readonly EntityKind Unnamed = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="EntityKind"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator EntityKind(string value) => new(value);
}
