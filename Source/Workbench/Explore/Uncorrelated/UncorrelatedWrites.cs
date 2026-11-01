// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Uncorrelated;

/// <summary>
/// The database writes that arrived with no HTTP command, grouped by table and by the hour of the day they happened in. A
/// job that runs every night shows up here as one table with transactions in one or two hours.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Tables">The tables written, in the order first seen.</param>
[ReadModel]
public record UncorrelatedWrites(PrologueId Id, IEnumerable<UncorrelatedTable> Tables)
{
    /// <summary>
    /// Observes the uncorrelated writes of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored writes.</param>
    /// <returns>The writes, pushed again whenever one arrives.</returns>
    public static ISubject<UncorrelatedWrites> ObserveUncorrelatedWrites(PrologueId id, IReadModels readModels) =>
        readModels.Observe<UncorrelatedWrites>(writes => writes.Id == id).FirstItem();

    /// <summary>
    /// Reduces a capture arriving. Only captures with no command and at least one transaction count.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static UncorrelatedWrites Reduce(CaptureRecorded @event, UncorrelatedWrites? current, EventContext context)
    {
        var writes = current ?? new(new PrologueId(Guid.Parse(context.EventSourceId.Value)), []);
        if (!@event.Http.Shape.IsNone || !@event.Transactions.Any())
        {
            return writes;
        }

        var hour = @event.Occurred.UtcDateTime.Hour;
        var tables = writes.Tables.ToList();
        foreach (var transaction in @event.Transactions)
        {
            foreach (var change in transaction.Tables.GroupBy(change => (change.Schema, change.Table)))
            {
                var index = tables.FindIndex(table => table.Write.Schema == change.Key.Schema && table.Write.Table == change.Key.Table);
                var existing = index < 0 ? new UncorrelatedTable(new TableWrite(change.Key.Schema, change.Key.Table, [], []), 0, []) : tables[index];
                var updated = existing with
                {
                    Write = TableWrites.Add([existing.Write], change).Single(),
                    Transactions = existing.Transactions + 1,
                    Hours = AddHour(existing.Hours, hour)
                };

                if (index < 0)
                {
                    tables.Add(updated);
                }
                else
                {
                    tables[index] = updated;
                }
            }
        }

        return writes with { Tables = tables };
    }

    static IEnumerable<HourCount> AddHour(IEnumerable<HourCount> hours, int hour)
    {
        var counts = hours.ToDictionary(entry => entry.Hour, entry => entry.Transactions);
        counts[hour] = counts.GetValueOrDefault(hour) + 1;
        return [.. counts.OrderBy(entry => entry.Key).Select(entry => new HourCount(entry.Key, entry.Value))];
    }
}
