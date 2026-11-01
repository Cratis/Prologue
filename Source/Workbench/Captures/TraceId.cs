// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// The trace identity a request and its spans share; none when the request carried no trace.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record TraceId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly TraceId None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="TraceId"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator TraceId(string value) => new(value);
}
