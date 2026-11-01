// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Relates the table changes of a transaction to the events the heuristics name for them, the way the heuristics do.
/// </summary>
public static class TableEvents
{
    /// <summary>
    /// Gets the events a transaction evidences, each with the change that evidences it. An update of a row inserted in the same transaction
    /// belongs to the insert, so a table is one event per kind of change.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The mechanical event name with its change.</returns>
    public static IEnumerable<(string Event, TableChange Change)> In(DatabaseTransactionObserved transaction) =>
        transaction.Tables
            .GroupBy(table => (table.Schema, table.Table))
            .SelectMany(group => group
                .GroupBy(table => table.Operation == ChangeOperation.Update && group.First().Operation == ChangeOperation.Insert ? ChangeOperation.Insert : table.Operation)
                .Select(changes => new TableChange(group.Key.Schema, group.Key.Table, changes.Key, [.. changes.SelectMany(table => table.Columns).Distinct(StringComparer.Ordinal)])))
            .Select(change => (Naming.EventName(Entity(change.Table), change.Operation), change));

    /// <summary>
    /// Gets the entity a table holds, the way the heuristics name it.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>The singular PascalCase entity name.</returns>
    public static string Entity(string table) => Naming.Singularize(Naming.Pascalize(table));

    /// <summary>
    /// Gets the stream a mechanical event name belongs to: the entity before Created, Updated, Deleted or Changed.
    /// </summary>
    /// <param name="mechanical">The mechanical event name.</param>
    /// <returns>The stream.</returns>
    public static StreamName StreamOf(string mechanical)
    {
        foreach (var suffix in new[] { "Created", "Updated", "Deleted", "Changed" })
        {
            if (mechanical.EndsWith(suffix, StringComparison.Ordinal) && mechanical.Length > suffix.Length)
            {
                return mechanical[..^suffix.Length];
            }
        }

        return mechanical;
    }
}
