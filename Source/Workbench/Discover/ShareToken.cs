// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover;

/// <summary>
/// The secret in a link that shows a prologue's map read-only.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ShareToken(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the token representing no share.
    /// </summary>
    public static readonly ShareToken None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ShareToken"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ShareToken(string value) => new(value);

    /// <summary>
    /// Creates an unguessable token.
    /// </summary>
    /// <returns>A new <see cref="ShareToken"/>.</returns>
    public static ShareToken New() =>
        new(Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24)).Replace('+', '-').Replace('/', '_').TrimEnd('='));
}
