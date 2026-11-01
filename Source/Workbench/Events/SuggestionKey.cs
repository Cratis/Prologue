// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events;

/// <summary>
/// Identifies a suggested event across interpretations: the module and the mechanical name the heuristics gave it, so the same evidence is the same suggestion however often the interpreter runs and whatever the language model names it. For example Catalog/BookCreated.
/// </summary>
/// <param name="Value">The value.</param>
public record SuggestionKey(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SuggestionKey"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator SuggestionKey(string value) => new(value);
}
