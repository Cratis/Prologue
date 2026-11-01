// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events;

/// <summary>
/// How strongly the evidence supports a suggestion, from 0 to 1.
/// </summary>
/// <param name="Value">The value.</param>
public record Confidence(double Value) : ConceptAs<double>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly Confidence None = new(0.0);

    /// <summary>
    /// Implicitly converts a <see cref="double"/> to a <see cref="Confidence"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Confidence(double value) => new(value);
}
