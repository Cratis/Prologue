// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Scenarios;

/// <summary>
/// The name of a business scenario a person runs while captures are watched, such as Lend a copy.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ScenarioName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly ScenarioName None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ScenarioName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ScenarioName(string value) => new(value);
}
