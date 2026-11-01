// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.given;

/// <summary>
/// Builds captures for the specs of Explore and reduces them into the command matrix.
/// </summary>
public static class Captured
{
    /// <summary>
    /// Builds a capture of a command whose first span names the handler.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The path.</param>
    /// <param name="status">The status.</param>
    /// <param name="handler">The name of the first span; none when empty.</param>
    /// <param name="scenario">The scenario running at the time.</param>
    /// <param name="transactions">The database transactions.</param>
    /// <returns>The event.</returns>
    public static CaptureRecorded Command(string method, string path, int status, string handler = "", string scenario = "", params RecordedTransaction[] transactions) =>
        RecordedCaptures.Command(method, path, status, transactions, scenario) with
        {
            Spans = handler.Length == 0 ? [] : [new RecordedSpan(handler, ServiceName.Default, 2, 30)]
        };

    /// <summary>
    /// Builds a transaction that changes several tables at once.
    /// </summary>
    /// <param name="changes">The changes, as operation, table and columns.</param>
    /// <returns>The transaction.</returns>
    public static RecordedTransaction Transaction(params (ChangeOperation Operation, string Table, string[] Columns)[] changes) =>
        new([.. changes.Select(change => new RecordedTableChange("dbo", new Systems.TableName(change.Table), change.Operation, [.. change.Columns.Select(column => new ColumnName(column))]))], 120);

    /// <summary>
    /// Reduces captures into the command matrix of a prologue.
    /// </summary>
    /// <param name="events">The captures, in the order they arrived.</param>
    /// <returns>The matrix.</returns>
    public static CommandMatrix Matrix(params CaptureRecorded[] events)
    {
        var context = EventContext.EmptyWithEventSourceId(PrologueId.New());
        CommandMatrix? matrix = null;
        foreach (var @event in events)
        {
            matrix = CommandMatrix.Reduce(@event, matrix, context);
        }

        return matrix!;
    }
}
#endif
