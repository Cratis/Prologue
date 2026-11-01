// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Explore.Journeys;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Observations;
using Cratis.Prologue.Workbench.Explore.Scope;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;

namespace Cratis.Prologue.Workbench.Explore.Asking;

/// <summary>
/// Summarizes what was captured into the compact, bounded text an agent answers questions from. It holds counts, shares, table
/// and column names and the shapes of commands; Prologue never has a row value, so there is none to leave out.
/// </summary>
public static class CaptureEvidence
{
    /// <summary>
    /// The most command shapes in the summary, the most captured first.
    /// </summary>
    public const int ShapesKept = 30;

    /// <summary>
    /// The most journeys in the summary.
    /// </summary>
    public const int JourneysKept = 8;

    /// <summary>
    /// The most characters in the summary; whatever does not fit is left out.
    /// </summary>
    public const int MaximumCharacters = 9000;

    /// <summary>
    /// Gets the command shapes the summary covers: the ones captured, without the ones left out of interpretation.
    /// </summary>
    /// <param name="matrix">The command matrix.</param>
    /// <param name="scope">The interpretation scope.</param>
    /// <returns>The shapes, the most captured first.</returns>
    public static IEnumerable<ShapeProfile> Shapes(CommandMatrix? matrix, InterpretationScope? scope) =>
        (matrix?.Shapes ?? [])
            .Where(profile => InterpretationScopes.Includes(scope, profile.Shape))
            .OrderByDescending(profile => profile.Captures)
            .Take(ShapesKept);

    /// <summary>
    /// Summarizes the captures.
    /// </summary>
    /// <param name="matrix">The command matrix.</param>
    /// <param name="uncorrelated">The writes with no command.</param>
    /// <param name="journeys">The inferred journeys.</param>
    /// <param name="scope">The interpretation scope.</param>
    /// <returns>The summary, at most <see cref="MaximumCharacters"/> long.</returns>
    public static string Summarize(CommandMatrix? matrix, UncorrelatedWrites? uncorrelated, Journeys.Journeys? journeys, InterpretationScope? scope)
    {
        var text = new StringBuilder();
        text.AppendLine("Command shapes (shape | captures | answers | handler | writes with the share of runs | scenarios):");
        foreach (var profile in Shapes(matrix, scope))
        {
            text.AppendLine(Line(profile));
        }

        AppendUncorrelated(text, uncorrelated);
        AppendJourneys(text, journeys);
        return text.Length <= MaximumCharacters ? text.ToString() : text.ToString(0, MaximumCharacters);
    }

    static string Line(ShapeProfile profile)
    {
        var answers = string.Join(", ", profile.Statuses.Take(6).Select(status => $"{status.Status.Value} {Share.Percent(status.Captures, profile.Captures)}%"));
        var handler = profile.Handlers.FirstOrDefault()?.Span.Value ?? "unknown";
        var writes = !profile.Writes.Any()
            ? "none"
            : string.Join("; ", profile.Writes.Take(8).Select(write => Write(write, profile.Captures)));
        var scenarios = string.Join(", ", profile.Scenarios.Take(3).Select(scenario => $"{scenario.Scenario.Value} {scenario.Captures}"));
        return $"- {profile.Shape.Value} | {profile.Captures} | {answers} | {handler} | {writes} | {(scenarios.Length == 0 ? "none" : scenarios)}";
    }

    static string Write(TableWrite write, int captures)
    {
        var operations = string.Join(", ", write.Operations.Select(runs => $"{runs.Operation.ToString().ToLowerInvariant()} {Share.Percent(runs.Runs, captures)}%"));
        return $"{write.Table.Value} {operations} ({string.Join(", ", write.Columns.Take(8).Select(column => column.Value))})";
    }

    static void AppendUncorrelated(StringBuilder text, UncorrelatedWrites? uncorrelated)
    {
        if (uncorrelated?.Tables.Any() != true)
        {
            return;
        }

        text.AppendLine("Database writes with no command (table | transactions | operations | hours of the day, UTC):");
        foreach (var table in uncorrelated.Tables.OrderByDescending(entry => entry.Transactions).Take(8))
        {
            var operations = string.Join(", ", table.Write.Operations.Select(runs => $"{runs.Operation.ToString().ToLowerInvariant()} {runs.Runs}"));
            var hours = string.Join(", ", table.Hours.Select(hour => $"{hour.Hour:00}:00 x{hour.Transactions}"));
            text.AppendLine($"- {table.Write.Table.Value} | {table.Transactions} | {operations} | {hours}");
        }
    }

    static void AppendJourneys(StringBuilder text, Journeys.Journeys? journeys)
    {
        if (journeys?.Sequences.Any() != true)
        {
            return;
        }

        text.AppendLine("Journeys of things identified in request paths (sequence | things | share of its kind | unfinished):");
        foreach (var sequence in journeys.Sequences.OrderByDescending(entry => entry.Entities).Take(JourneysKept))
        {
            var unfinished = sequence.ContinuedBy.IsNone ? "no" : $"others went on to {sequence.ContinuedBy.Value}";
            text.AppendLine($"- {sequence.Kind.Value}: {string.Join(" -> ", sequence.Steps.Select(step => step.Value))} | {sequence.Entities} | {sequence.Share}% | {unfinished}");
        }
    }
}
