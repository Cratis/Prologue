// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover;

/// <summary>
/// The name of something a system can do, such as "Lend a copy".
/// </summary>
/// <param name="Value">The underlying value.</param>
public record CapabilityName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="CapabilityName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CapabilityName(string value) => new(value);
}
