// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Explore.Matrix;

namespace Cratis.Prologue.Workbench.Explore.Observations;

/// <summary>
/// Notices what is worth telling a person about a command shape, from the numbers captured for it.
/// </summary>
public static class ShapeObservations
{
    /// <summary>
    /// Finds the observations about a command shape.
    /// </summary>
    /// <param name="profile">What was captured about the shape.</param>
    /// <returns>The observations, the ones about the answers first.</returns>
    public static IEnumerable<ShapeObservation> Of(ShapeProfile profile)
    {
        var splitTables = profile.Writes.Where(write => SplitByStatus(profile, write) is not null).ToList();
        return
        [
            .. splitTables.Select(write => InsertsThenUpdates(profile, write)),
            .. AnswersWithoutWriting(profile),
            .. WritesInSomeRuns(profile, splitTables)
        ];
    }

    static (StatusProfile Inserting, StatusProfile Updating)? SplitByStatus(ShapeProfile profile, TableWrite write)
    {
        var inserting = profile.Statuses.FirstOrDefault(status => Only(status, write, ChangeOperation.Insert));
        var updating = profile.Statuses.FirstOrDefault(status => Only(status, write, ChangeOperation.Update));
        return inserting is not null && updating is not null ? (inserting, updating) : null;
    }

    static ShapeObservation InsertsThenUpdates(ShapeProfile profile, TableWrite write)
    {
        var (inserting, updating) = SplitByStatus(profile, write)!.Value;
        return new(
            ObservationKind.InsertsThenUpdates,
            $"The same route inserts into {write.Table.Value} on first use ({inserting.Status.Value}) and updates it afterwards ({updating.Status.Value}). That is two behaviors behind one route.");
    }

    static bool Only(StatusProfile status, TableWrite table, ChangeOperation operation)
    {
        var operations = status.Writes
            .Where(write => write.Schema == table.Schema && write.Table == table.Table)
            .SelectMany(write => write.Operations)
            .Select(runs => runs.Operation)
            .ToList();
        return operations.Contains(operation) && operations.TrueForAll(seen => seen == operation);
    }

    static IEnumerable<ShapeObservation> AnswersWithoutWriting(ShapeProfile profile) =>
        profile.Statuses
            .Where(status => status.Status.Value >= 400 && status.WithoutWrites > 0)
            .Select(status => new ShapeObservation(
                ObservationKind.AnswersWithoutWriting,
                $"{Share.Percent(status.WithoutWrites, profile.Captures)}% of runs answer {status.Status.Value} and write nothing."));

    static IEnumerable<ShapeObservation> WritesInSomeRuns(ShapeProfile profile, List<TableWrite> alreadyExplained)
    {
        var writing = profile.Captures - profile.WithoutWrites;
        return profile.Writes
            .Except(alreadyExplained)
            .SelectMany(write => write.Operations
                .Where(runs => runs.Runs < writing)
                .Select(runs => new ShapeObservation(
                    ObservationKind.WritesInSomeRuns,
                    $"Writes {write.Table.Value} only in some runs: {Word(runs.Operation)} in {Share.Percent(runs.Runs, profile.Captures)}% of runs.")));
    }

    static string Word(ChangeOperation operation) => operation switch
    {
        ChangeOperation.Insert => "insert",
        ChangeOperation.Update => "update",
        _ => "delete"
    };
}
