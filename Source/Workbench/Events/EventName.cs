// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events;

/// <summary>
/// The name of an event: the mechanical name the heuristics gave it, the name the language model refined it to, or the name a person chose.
/// </summary>
/// <param name="Value">The value.</param>
public record EventName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly EventName None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="EventName"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator EventName(string value) => new(value);
}
