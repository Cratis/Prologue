// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// The name of the capability on the application map a decision belongs to.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record CapabilityName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly CapabilityName None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="CapabilityName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CapabilityName(string value) => new(value);
}
