// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The name of a database table allowed for capture.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record TableName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="TableName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator TableName(string value) => new(value);
}
