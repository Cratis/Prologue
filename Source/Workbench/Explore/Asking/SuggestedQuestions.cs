// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Observations;
using Cratis.Prologue.Workbench.Explore.Scope;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;

namespace Cratis.Prologue.Workbench.Explore.Asking;

/// <summary>
/// Works out which questions are worth asking about the captures, and what the numbers answer without an agent.
/// </summary>
public static class SuggestedQuestions
{
    const int CitationsKept = 6;

    /// <summary>
    /// Suggests questions from what was captured: what the most captured commands do, which commands change the most written
    /// table, why commands are refused, and what writes happen outside any command. A question is only suggested when the
    /// captures have something to say about it.
    /// </summary>
    /// <param name="matrix">The command matrix.</param>
    /// <param name="uncorrelated">The writes with no command.</param>
    /// <param name="scope">The interpretation scope; excluded shapes are not asked about.</param>
    /// <returns>The suggestions.</returns>
    public static IEnumerable<SuggestedQuestion> For(CommandMatrix? matrix, UncorrelatedWrites? uncorrelated, InterpretationScope? scope)
    {
        var shapes = CaptureEvidence.Shapes(matrix, scope).ToList();
        return
        [
            .. shapes.Take(2).Select(What),
            .. WhoChanges(shapes),
            .. WhyRefused(shapes),
            .. Outside(shapes, uncorrelated)
        ];
    }

    static SuggestedQuestion What(ShapeProfile profile)
    {
        var answers = string.Join(" and ", profile.Statuses.Take(3).Select(status => $"{status.Status.Value} in {Share.Percent(status.Captures, profile.Captures)}%"));
        var writes = profile.Writes.Any()
            ? $" It writes {string.Join(", ", profile.Writes.Select(write => $"{write.Table.Value} ({string.Join('/', write.Operations.Select(runs => $"{Word(runs.Operation)} {Share.Percent(runs.Runs, profile.Captures)}%"))})"))}."
            : " It writes no table.";
        var notes = profile.Observations.Any() ? $" {string.Join(' ', profile.Observations.Select(observation => observation.Text))}" : string.Empty;
        return new(
            $"What happens when {profile.Shape.Value} runs?",
            $"{profile.Shape.Value} was captured {profile.Captures} times and answered {answers}.{writes}{notes}",
            [profile.Shape]);
    }

    static IEnumerable<SuggestedQuestion> WhoChanges(List<ShapeProfile> shapes)
    {
        var busiest = shapes
            .SelectMany(profile => profile.Writes.Select(write => (profile.Shape, Write: write)))
            .GroupBy(entry => entry.Write.Table)
            .OrderByDescending(group => group.Count())
            .FirstOrDefault();
        if (busiest is null)
        {
            return [];
        }

        var changers = busiest.Take(CitationsKept).ToList();
        var listing = string.Join("; ", changers.Select(entry => $"{entry.Shape.Value} ({string.Join('/', entry.Write.Operations.Select(runs => Word(runs.Operation)))})"));
        return
        [
            new(
                $"Which commands change {busiest.Key.Value}?",
                $"{busiest.Count()} captured command shapes change {busiest.Key.Value}: {listing}.",
                [.. changers.Select(entry => entry.Shape)])
        ];
    }

    static IEnumerable<SuggestedQuestion> WhyRefused(List<ShapeProfile> shapes)
    {
        var refusing = shapes
            .Select(profile => (profile.Shape, Notes: profile.Observations.Where(observation => observation.Kind == ObservationKind.AnswersWithoutWriting).ToList()))
            .Where(entry => entry.Notes.Count > 0)
            .Take(CitationsKept)
            .ToList();
        return refusing.Count == 0
            ? []
            :
            [
                new(
                    "Why are some commands refused?",
                    string.Join(' ', refusing.Select(entry => $"{entry.Shape.Value}: {string.Join(' ', entry.Notes.Select(note => note.Text))}")) + " Each is a candidate validation rule or constraint.",
                    [.. refusing.Select(entry => entry.Shape)])
            ];
    }

    static IEnumerable<SuggestedQuestion> Outside(List<ShapeProfile> shapes, UncorrelatedWrites? uncorrelated)
    {
        if (uncorrelated?.Tables.Any() != true)
        {
            return [];
        }

        var listing = string.Join("; ", uncorrelated.Tables.Select(table => $"{table.Write.Table.Value} in {table.Transactions} transactions around {string.Join(", ", table.Hours.Select(hour => $"{hour.Hour:00}:00"))} UTC"));
        var alsoByCommands = shapes
            .Where(profile => profile.Writes.Any(write => uncorrelated.Tables.Any(table => table.Write.Table == write.Table)))
            .Take(CitationsKept)
            .Select(profile => profile.Shape)
            .ToList();
        var also = alsoByCommands.Count > 0 ? $" Commands that also write these tables: {string.Join(", ", alsoByCommands.Select(shape => shape.Value))}." : string.Empty;
        return [new("Which writes happen outside any command?", $"Database writes with no command: {listing}.{also}", alsoByCommands)];
    }

    static string Word(ChangeOperation operation) => operation switch
    {
        ChangeOperation.Insert => "insert",
        ChangeOperation.Update => "update",
        _ => "delete"
    };
}
