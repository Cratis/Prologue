// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events;

/// <summary>
/// The stream an event belongs to, named after the entity whose life it is part of, for example Book.
/// </summary>
/// <param name="Value">The value.</param>
public record StreamName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="StreamName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator StreamName(string value) => new(value);
}
