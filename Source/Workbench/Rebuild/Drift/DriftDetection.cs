// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Captures.Recording;

namespace Cratis.Prologue.Workbench.Rebuild.Drift;

/// <summary>
/// Learns what the legacy system does before the model is accepted and spots what it does differently afterwards.
/// </summary>
public static class DriftDetection
{
    /// <summary>
    /// Folds a capture into the drift. Captures with no command belong to no shape and are not drift.
    /// </summary>
    /// <param name="drift">The drift so far.</param>
    /// <param name="capture">The capture.</param>
    /// <returns>The drift with the capture folded in.</returns>
    public static ModelDrift Observe(ModelDrift drift, CaptureRecorded capture)
    {
        if (capture.Http.Shape.IsNone)
        {
            return drift;
        }

        var writes = WritesOf(capture).ToList();
        return drift.Accepted ? Compare(drift, capture, writes) : Learn(drift, capture.Http.Shape, writes);
    }

    /// <summary>
    /// Gets the columns a capture's transactions wrote.
    /// </summary>
    /// <param name="capture">The capture.</param>
    /// <returns>The columns written, once each.</returns>
    public static IEnumerable<WrittenColumn> WritesOf(CaptureRecorded capture) =>
        capture.Transactions
            .SelectMany(transaction => transaction.Tables)
            .SelectMany(change => change.Columns.Any()
                ? change.Columns.Select(column => new WrittenColumn($"{change.Schema.Value}.{change.Table.Value}.{column.Value}"))
                : [new WrittenColumn($"{change.Schema.Value}.{change.Table.Value}.*")])
            .Distinct();

    static ModelDrift Learn(ModelDrift drift, CommandShape shape, List<WrittenColumn> writes)
    {
        var known = drift.Known.FirstOrDefault(entry => entry.Shape == shape);
        var learned = new KnownShape(shape, [.. (known?.Writes ?? []).Union(writes)]);
        return drift with { Known = [.. drift.Known.Where(entry => entry.Shape != shape), learned] };
    }

    static ModelDrift Compare(ModelDrift drift, CaptureRecorded capture, List<WrittenColumn> writes)
    {
        var shape = capture.Http.Shape;
        var known = drift.Known.FirstOrDefault(entry => entry.Shape == shape);
        if (known is null)
        {
            return Seen(drift, DriftKey.ForShape(shape), DriftKind.NewShape, shape, WrittenColumn.None, capture.Occurred);
        }

        return writes
            .Where(column => !known.Writes.Contains(column))
            .Aggregate(drift, (current, column) => Seen(current, DriftKey.ForColumn(shape, column), DriftKind.NewColumn, shape, column, capture.Occurred));
    }

    static ModelDrift Seen(ModelDrift drift, DriftKey key, DriftKind kind, CommandShape shape, WrittenColumn column, DateTimeOffset at)
    {
        var existing = drift.Items.FirstOrDefault(item => item.Key == key);
        return existing is null
            ? drift with { Items = [.. drift.Items, new DriftItem(key, kind, shape, column, 1, at, false)] }
            : drift with { Items = [.. drift.Items.Select(item => item.Key == key ? item with { Captures = item.Captures + 1, LastSeen = at } : item)] };
    }
}
