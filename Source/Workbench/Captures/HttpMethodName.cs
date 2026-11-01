// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// The upper-cased HTTP method of an observed command, such as POST.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record HttpMethodName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly HttpMethodName None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="HttpMethodName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator HttpMethodName(string value) => new(value);
}
