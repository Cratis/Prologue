// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using TableName = Cratis.Prologue.Workbench.Systems.TableName;

namespace Cratis.Prologue.Workbench.Explore.Matrix;

/// <summary>
/// Folds the changes of one run into what has been written to each table so far.
/// </summary>
public static class TableWrites
{
    /// <summary>
    /// Adds the changes of one run. A kind of change counts once per run however many rows it touched, and the columns join those seen before.
    /// </summary>
    /// <param name="current">What was written so far.</param>
    /// <param name="changes">The table changes of the run.</param>
    /// <returns>What has been written, tables in the order first seen.</returns>
    public static IEnumerable<TableWrite> Add(IEnumerable<TableWrite> current, IEnumerable<RecordedTableChange> changes)
    {
        var writes = current.ToList();
        foreach (var group in changes.GroupBy(change => (change.Schema, change.Table)))
        {
            var index = writes.FindIndex(write => write.Schema == group.Key.Schema && write.Table == group.Key.Table);
            var existing = index < 0 ? new TableWrite(group.Key.Schema, group.Key.Table, [], []) : writes[index];
            var updated = existing with
            {
                Operations = Count(existing.Operations, group.Select(change => change.Operation).Distinct()),
                Columns = [.. existing.Columns.Union(group.SelectMany(change => change.Columns))]
            };

            if (index < 0)
            {
                writes.Add(updated);
            }
            else
            {
                writes[index] = updated;
            }
        }

        return writes;
    }

    static IEnumerable<OperationRuns> Count(IEnumerable<OperationRuns> current, IEnumerable<ChangeOperation> seen)
    {
        var runs = current.ToDictionary(entry => entry.Operation, entry => entry.Runs);
        foreach (var operation in seen)
        {
            runs[operation] = runs.GetValueOrDefault(operation) + 1;
        }

        return [.. runs.OrderBy(entry => entry.Key).Select(entry => new OperationRuns(entry.Key, entry.Value))];
    }
}
