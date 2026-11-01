// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The name of an HTTP cluster a system is reached through.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ClusterName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ClusterName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ClusterName(string value) => new(value);
}
