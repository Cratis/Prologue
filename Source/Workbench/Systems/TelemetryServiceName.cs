// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The name an OpenTelemetry service reports itself with.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record TelemetryServiceName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="TelemetryServiceName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator TelemetryServiceName(string value) => new(value);
}
