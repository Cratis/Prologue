// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Scope;

namespace Cratis.Prologue.Workbench.Events.NotEvents;

/// <summary>
/// Derives what the evidence shows is not an event: values kept up to date as a side effect of other commands, nullable columns that are set later
/// by a command of their own, and the traffic that was left out. Pure.
/// </summary>
public static partial class NotEventDetection
{
    [GeneratedRegex("count|total|balance|available|remaining|quantity|sum$|stock", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Counter { get; }

    /// <summary>
    /// Derives the things that are not events.
    /// </summary>
    /// <param name="facts">What is known about the prologue.</param>
    /// <param name="scope">The interpretation scope, or null when nothing was ever excluded or split.</param>
    /// <returns>The derived state, the nullable facts and the excluded traffic, in that order.</returns>
    public static IReadOnlyList<NotEvent> Detect(EvidenceFacts facts, InterpretationScope? scope)
    {
        var shapes = facts.Shapes.Where(profile => InterpretationScopes.Includes(scope, profile.Shape)).ToList();
        var derived = DerivedState(shapes);
        var derivedSubjects = derived.Select(item => item.Subject).ToHashSet();
        return
        [
            .. derived,
            .. NullableFacts(shapes, facts).Where(item => !derivedSubjects.Contains(item.Subject)),
            .. (scope?.Excluded ?? []).OrderBy(shape => shape.Value, StringComparer.Ordinal)
                .Select(shape => new NotEvent(NotEventKind.ExcludedTraffic, shape.Value, "Left out of the interpretation in Explore, for example load-generator traffic."))
        ];
    }

    static IEnumerable<NotEvent> DerivedState(List<ShapeProfile> shapes)
    {
        var updates = shapes
            .SelectMany(profile => profile.Statuses.Where(status => status.Status.Value < 400).SelectMany(status => status.Writes.Select(write => (Profile: profile, Write: write, Others: status.Writes.Count(other => other != write)))))
            .Where(item => item.Write.Operations.Any(run => run.Operation == ChangeOperation.Update))
            .SelectMany(item => item.Write.Columns.Select(column => (item.Profile.Shape, Table: item.Write.Table.Value, Column: column.Value, item.Others)))
            .GroupBy(item => (item.Table, item.Column));

        foreach (var group in updates.OrderBy(group => group.Key.Table, StringComparer.Ordinal).ThenBy(group => group.Key.Column, StringComparer.Ordinal))
        {
            var sideEffect = group.Where(item => item.Others > 0).Select(item => item.Shape).Distinct().ToList();
            if (sideEffect.Count >= 2 || (sideEffect.Count == 1 && Counter.IsMatch(group.Key.Column)))
            {
                yield return new NotEvent(
                    NotEventKind.DerivedState,
                    $"{group.Key.Table}.{group.Key.Column}",
                    $"Changes as a side effect of {string.Join(", ", sideEffect.Select(shape => shape.Value))}. Derived state: it becomes a read model, not an event property.");
            }
        }
    }

    static IEnumerable<NotEvent> NullableFacts(List<ShapeProfile> shapes, EvidenceFacts facts)
    {
        var writes = shapes.SelectMany(profile => profile.Statuses.Where(status => status.Status.Value < 400).SelectMany(status => status.Writes.Select(write => (profile.Shape, Write: write)))).ToList();
        foreach (var table in facts.Schema.OrderBy(table => table.Table, StringComparer.Ordinal))
        {
            var tableWrites = writes.Where(item => string.Equals(item.Write.Table.Value, table.Table, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var column in table.Columns.Where(column => column.IsNullable && !table.PrimaryKey.Contains(column.Name)))
            {
                var insertsIt = tableWrites.Exists(item => item.Write.Operations.Any(run => run.Operation == ChangeOperation.Insert) && item.Write.Columns.Any(candidate => candidate.Value == column.Name));
                var setBy = tableWrites
                    .Where(item => item.Write.Operations.Any(run => run.Operation == ChangeOperation.Update) && item.Write.Columns.Any(candidate => candidate.Value == column.Name))
                    .Select(item => item.Shape.Value)
                    .Distinct()
                    .ToList();
                if (!insertsIt && setBy.Count > 0)
                {
                    yield return new NotEvent(
                        NotEventKind.NullableFact,
                        $"{table.Table}.{column.Name}",
                        $"Nullable column, set later by {string.Join(", ", setBy)}. Recorded as its own fact rather than an optional property of the event that created the row.");
                }
            }
        }
    }
}
