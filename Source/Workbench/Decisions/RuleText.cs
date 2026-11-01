// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// A business rule in the words it was found in or observed as.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record RuleText(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets the value representing none.
    /// </summary>
    public static readonly RuleText None = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="RuleText"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator RuleText(string value) => new(value);
}
