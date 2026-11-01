// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery;

/// <summary>
/// A business rule as the code enforces it, in a sentence.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record RuleText(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="RuleText"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator RuleText(string value) => new(value);
}
