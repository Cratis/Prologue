// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover;

/// <summary>
/// The shape of an HTTP command, as "METHOD /path" with every identifying segment replaced by {id}, such as "POST /api/loans/{id}/return".
/// </summary>
/// <param name="Value">The underlying value.</param>
public record RouteShape(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the shape representing no route, for a finding the code does not tie to a command.
    /// </summary>
    public static readonly RouteShape None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="RouteShape"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator RouteShape(string value) => new(value);
}
