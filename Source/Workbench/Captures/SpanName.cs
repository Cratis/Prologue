// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// The name of an observed telemetry span.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SpanName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly SpanName None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SpanName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SpanName(string value) => new(value);
}
