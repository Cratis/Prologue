// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Explore.Matrix;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Derives the notes on a suggested event: the constraints a unique index suggests, the rules refusals and the code suggest, and what is worth knowing.
/// </summary>
public static class SuggestionNotes
{
    /// <summary>
    /// Gets whether two command shapes are the same, ignoring case.
    /// </summary>
    /// <param name="shape">One shape.</param>
    /// <param name="other">The other.</param>
    /// <returns>True when they are.</returns>
    public static bool Same(CommandShape shape, CommandShape other) => string.Equals(shape.Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Derives the notes for an event.
    /// </summary>
    /// <param name="attribution">What the captures show about the event.</param>
    /// <param name="facts">What else is known about the prologue.</param>
    /// <returns>The notes, constraints first.</returns>
    public static IReadOnlyList<SuggestionNote> For(Attribution attribution, EvidenceFacts facts) =>
    [
        .. Constraints(attribution, facts),
        .. Refusals(attribution, facts),
        .. Code(attribution, facts),
        .. Occasional(attribution, facts)
    ];

    static IEnumerable<SuggestionNote> Constraints(Attribution attribution, EvidenceFacts facts)
    {
        foreach (var ((schema, table, operation), columns) in attribution.Tables.Where(entry => entry.Key.Operation == nameof(ChangeOperation.Insert)))
        {
            var found = facts.Schema.FirstOrDefault(candidate => candidate.Table == table && candidate.Schema == schema);
            foreach (var unique in found?.UniqueConstraints.Where(unique => unique.Columns.Count > 0 && unique.Columns.All(columns.Contains)) ?? [])
            {
                var on = string.Join(", ", unique.Columns);
                yield return new(NoteKind.Constraint, $"{table}.{on} has a unique index. Suggested constraint: one {TableEvents.Entity(table)} per {on}.");
            }
        }
    }

    static IEnumerable<SuggestionNote> Refusals(Attribution attribution, EvidenceFacts facts)
    {
        foreach (var shape in attribution.Shapes.Keys)
        {
            var profile = facts.Shapes.FirstOrDefault(candidate => candidate.Shape == shape);
            var refused = profile?.Statuses.Where(status => status.Status.Value >= 400 && !status.Writes.Any()).ToList() ?? [];
            if (refused.Count == 0 || profile is null)
            {
                continue;
            }

            var count = refused.Sum(status => status.Captures);
            var codes = string.Join(", ", refused.Select(status => status.Status.Value).Distinct().Order());
            yield return new(NoteKind.Rule, $"{shape.Value} is refused with {codes} in {count} of {profile.Captures} captures. The event only happens when the command is not refused: find the rule that decides it.");
        }
    }

    static IEnumerable<SuggestionNote> Code(Attribution attribution, EvidenceFacts facts) =>
        facts.CodePoints
            .Where(point => point.Outcome == DecisionOutcome.Refused && attribution.Shapes.Keys.Any(shape => Same(shape, point.Shape)))
            .Select(point => new SuggestionNote(NoteKind.Rule, $"Code refuses: {point.Rule.Value} ({point.File.Value}:{point.Line.Value}). Suggested rule on the command."));

    static IEnumerable<SuggestionNote> Occasional(Attribution attribution, EvidenceFacts facts)
    {
        foreach (var (shape, captures) in attribution.Shapes)
        {
            var total = facts.Shapes.FirstOrDefault(candidate => candidate.Shape == shape)?.Captures ?? 0;
            var withWrites = total - (facts.Shapes.FirstOrDefault(candidate => candidate.Shape == shape)?.WithoutWrites ?? 0);
            if (withWrites > 0 && captures * 2 < withWrites)
            {
                yield return new(NoteKind.Observation, $"Only {captures} of {withWrites} captures of {shape.Value} that wrote anything show this event. It may be a different outcome of the same command.");
            }
        }
    }
}
