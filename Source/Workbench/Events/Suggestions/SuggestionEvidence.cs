// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// What a suggestion rests on: the captured command shapes, the table changes and the places in the code.
/// </summary>
/// <param name="Shapes">The command shapes and how many captures of each evidence the suggestion.</param>
/// <param name="Tables">The table changes the captures show.</param>
/// <param name="Code">The places in the code Discover found.</param>
/// <param name="Scenarios">The scenario labels the evidencing captures were recorded under.</param>
public record SuggestionEvidence(
    IEnumerable<ShapeEvidence> Shapes,
    IEnumerable<TableEvidence> Tables,
    IEnumerable<CodeEvidence> Code,
    IEnumerable<string> Scenarios)
{
    /// <summary>
    /// Gets the number of captures behind the suggestion.
    /// </summary>
    public int Captures => Shapes.Sum(shape => shape.Captures);
}
