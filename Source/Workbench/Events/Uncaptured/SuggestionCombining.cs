// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Uncaptured;

/// <summary>
/// Combines two suggestions that ended up with the same key into one, so the second one's evidence is kept instead of silently dropped.
/// </summary>
public static class SuggestionCombining
{
    /// <summary>
    /// Combines the suggestion found first with one found later under the same key.
    /// </summary>
    /// <param name="first">The suggestion found first; it decides the names and the origin.</param>
    /// <param name="later">The suggestion found later; its evidence, notes and properties are added.</param>
    /// <returns>The combined suggestion.</returns>
    public static SuggestedEvent Combine(SuggestedEvent first, SuggestedEvent later) =>
        first with
        {
            Properties = [.. first.Properties.Concat(later.Properties).DistinctBy(property => property.Name, StringComparer.OrdinalIgnoreCase)],
            Confidence = first.Confidence.Value >= later.Confidence.Value ? first.Confidence : later.Confidence,
            Evidence = new SuggestionEvidence(
                [.. first.Evidence.Shapes.Concat(later.Evidence.Shapes).DistinctBy(shape => shape.Shape.Value, StringComparer.OrdinalIgnoreCase)],
                [.. first.Evidence.Tables.Concat(later.Evidence.Tables).DistinctBy(table => $"{table.Schema}.{table.Table}.{table.Operation}", StringComparer.OrdinalIgnoreCase)],
                [.. first.Evidence.Code.Concat(later.Evidence.Code).Distinct()],
                [.. first.Evidence.Scenarios.Concat(later.Evidence.Scenarios).Distinct()]),
            Notes = [.. first.Notes.Concat(later.Notes).DistinctBy(note => note.Text)]
        };
}
