// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the name of the service an observation was captured from. A system made of several services can be
/// captured as named services, each with its own databases, HTTP traffic and telemetry; a system captured as one
/// service uses <see cref="Default"/>.
/// </summary>
/// <param name="Value">The underlying string value.</param>
public record ServiceName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents the default, unnamed service — what every capture belongs to when no named services are configured.
    /// </summary>
    public static readonly ServiceName Default = new(string.Empty);

    /// <summary>
    /// Gets a value indicating whether this is the <see cref="Default"/> service.
    /// </summary>
    public bool IsDefault => string.IsNullOrEmpty(Value);

    /// <summary>
    /// Implicitly converts a string to a <see cref="ServiceName"/>.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    public static implicit operator ServiceName(string value) => new(value);
}
