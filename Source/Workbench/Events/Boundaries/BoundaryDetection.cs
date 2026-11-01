// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Scope;

namespace Cratis.Prologue.Workbench.Events.Boundaries;

/// <summary>
/// Derives the consistency boundaries the evidence raises. Pure: the same evidence gives the same boundaries with the same keys, so a choice
/// made once is still the choice the next time they are derived.
/// </summary>
public static class BoundaryDetection
{
    /// <summary>
    /// The key of the option that keeps the things together in one stream.
    /// </summary>
    public const string OneStream = "one-stream";

    /// <summary>
    /// The key of the option that keeps separate streams and checks across them.
    /// </summary>
    public const string Dynamic = "dcb";

    /// <summary>
    /// The key of the option that allows the removal and keeps what refers to it.
    /// </summary>
    public const string KeepReferences = "keep-references";

    /// <summary>
    /// Derives the boundaries.
    /// </summary>
    /// <param name="facts">What is known about the prologue.</param>
    /// <param name="scope">The interpretation scope, or null when nothing was ever excluded or split.</param>
    /// <returns>The boundaries, in the order of the command shapes.</returns>
    public static IReadOnlyList<ConsistencyBoundary> Detect(EvidenceFacts facts, InterpretationScope? scope) =>
    [
        .. facts.Shapes
            .Where(profile => InterpretationScopes.Includes(scope, profile.Shape))
            .OrderBy(profile => profile.Shape.Value, StringComparer.Ordinal)
            .SelectMany(profile => Across(profile, facts).Concat(Removal(profile, facts)))
    ];

    static IEnumerable<ConsistencyBoundary> Across(ShapeProfile profile, EvidenceFacts facts)
    {
        var written = Succeeded(profile).SelectMany(status => status.Writes).GroupBy(write => (write.Schema.Value, write.Table.Value)).Select(group => group.First()).ToList();
        var entities = written.Select(write => TableEvents.Entity(write.Table.Value)).Distinct().ToList();
        var refused = Refused(profile);
        if (entities.Count < 2 || refused.Count == 0)
        {
            yield break;
        }

        var primary = TableEvents.Entity((written.FirstOrDefault(write => write.Operations.Any(run => run.Operation == ChangeOperation.Insert)) ?? written[0]).Table.Value);
        var others = string.Join(" and ", entities.Where(entity => entity != primary));
        var rule = CodeRule(profile, facts) ?? $"{profile.Shape.Value} only holds when {primary} and {others} change together";
        yield return new ConsistencyBoundary(
            $"{profile.Shape.Value}|across",
            profile.Shape,
            rule,
            $"{profile.Shape.Value} writes {string.Join(", ", written.Select(write => write.Table.Value))} in one transaction. Refused {Times(refused)} with {Codes(refused)}.",
            [
                new BoundaryOption(OneStream, $"Decide on the {primary} stream: {others} change together with it", $"{primary} is the consistency boundary of {profile.Shape.Value}: {rule}"),
                new BoundaryOption(Dynamic, $"Keep a stream for each and check {others} with a dynamic consistency boundary", $"{profile.Shape.Value} checks {others} with a dynamic consistency boundary: {rule}")
            ]);
    }

    static IEnumerable<ConsistencyBoundary> Removal(ShapeProfile profile, EvidenceFacts facts)
    {
        var refused = Refused(profile);
        if (refused.Count == 0)
        {
            yield break;
        }

        foreach (var removed in Succeeded(profile).SelectMany(status => status.Writes).Where(write => write.Operations.Any(run => run.Operation == ChangeOperation.Delete)).GroupBy(write => write.Table.Value).Select(group => group.Key))
        {
            var referencing = facts.Schema
                .Where(table => table.ForeignKeys.Any(key => string.Equals(key.ReferencedTable, removed, StringComparison.OrdinalIgnoreCase)))
                .Select(table => table.Table)
                .Distinct()
                .ToList();
            if (referencing.Count == 0)
            {
                continue;
            }

            var entity = TableEvents.Entity(removed);
            var by = string.Join(" and ", referencing.Select(TableEvents.Entity));
            var rule = CodeRule(profile, facts) ?? $"{entity} cannot be removed while {by} rows refer to it";
            yield return new ConsistencyBoundary(
                $"{profile.Shape.Value}|removal",
                profile.Shape,
                rule,
                $"{profile.Shape.Value} deletes {removed}, which {string.Join(", ", referencing)} reference. Refused {Times(refused)} with {Codes(refused)}.",
                [
                    new BoundaryOption(Dynamic, $"Check the {by} of the {entity} across streams at append time", $"{profile.Shape.Value} checks {by} with a dynamic consistency boundary: {rule}"),
                    new BoundaryOption(KeepReferences, $"Allow the removal and keep {by} pointing at a retired {entity}", $"{entity} can be removed: {by} keep pointing at a retired {entity}")
                ]);
        }
    }

    static string? CodeRule(ShapeProfile profile, EvidenceFacts facts) =>
        facts.CodePoints.FirstOrDefault(point => point.Outcome == DecisionOutcome.Refused && string.Equals(point.Shape.Value, profile.Shape.Value, StringComparison.OrdinalIgnoreCase))?.Rule.Value;

    static List<StatusProfile> Succeeded(ShapeProfile profile) => [.. profile.Statuses.Where(status => status.Status.Value < 400 && status.Writes.Any())];

    static List<StatusProfile> Refused(ShapeProfile profile) => [.. profile.Statuses.Where(status => status.Status.Value >= 400 && !status.Writes.Any())];

    static string Times(IEnumerable<StatusProfile> statuses) => statuses.Sum(status => status.Captures) is var count and not 1 ? $"{count} times" : "1 time";

    static string Codes(IEnumerable<StatusProfile> statuses) => string.Join(", ", statuses.Select(status => status.Status.Value).Distinct().Order());
}
