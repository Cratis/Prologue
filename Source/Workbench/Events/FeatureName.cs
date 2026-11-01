// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events;

/// <summary>
/// The feature a slice belongs to; nested features are joined with a slash.
/// </summary>
/// <param name="Value">The value.</param>
public record FeatureName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="FeatureName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator FeatureName(string value) => new(value);
}
