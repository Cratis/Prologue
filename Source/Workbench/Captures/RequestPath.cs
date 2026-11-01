// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// The path of an observed request, without its query string.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record RequestPath(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly RequestPath None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="RequestPath"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator RequestPath(string value) => new(value);
}
