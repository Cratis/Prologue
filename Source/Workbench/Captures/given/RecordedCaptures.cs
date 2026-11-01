// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.Live;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Scenarios;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Captures.given;

/// <summary>
/// Builds recorded captures for specs.
/// </summary>
public static class RecordedCaptures
{
    /// <summary>
    /// The time the first capture of a spec happened.
    /// </summary>
    public static readonly DateTimeOffset Start = new(2026, 11, 1, 12, 0, 20, TimeSpan.Zero);

    /// <summary>
    /// Builds a database transaction that changes one table.
    /// </summary>
    /// <param name="operation">What is done to a row.</param>
    /// <param name="table">The table.</param>
    /// <param name="columns">The columns changed.</param>
    /// <returns>The transaction.</returns>
    public static RecordedTransaction Write(ChangeOperation operation, string table, params string[] columns) =>
        new([new RecordedTableChange("dbo", new TableName(table), operation, [.. columns.Select(column => new ColumnName(column))])], 120);

    /// <summary>
    /// Builds a capture of a command.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The path.</param>
    /// <param name="status">The status the command was answered with.</param>
    /// <param name="transactions">The transactions that came with it.</param>
    /// <param name="scenario">The scenario running at the time, if any.</param>
    /// <param name="traceLinked">Whether the spans were joined by trace.</param>
    /// <param name="occurred">When it happened.</param>
    /// <returns>The event.</returns>
    public static CaptureRecorded Command(
        string method,
        string path,
        int status,
        RecordedTransaction[]? transactions = null,
        string scenario = "",
        bool traceLinked = false,
        DateTimeOffset? occurred = null) =>
        new(
            Guid.NewGuid(),
            occurred ?? Start,
            ServiceName.Default,
            new RecordedCommand(method, path, status, traceLinked ? "abc" : string.Empty, CommandShape.From(method, path), 0),
            transactions ?? [],
            2,
            [],
            traceLinked,
            new ScenarioName(scenario));

    /// <summary>
    /// Builds a capture with database transactions and no command.
    /// </summary>
    /// <param name="transactions">The transactions.</param>
    /// <returns>The event.</returns>
    public static CaptureRecorded WithoutCommand(params RecordedTransaction[] transactions) =>
        new(Guid.NewGuid(), Start, ServiceName.Default, RecordedCommand.None, transactions, 0, [], false, ScenarioName.None);
}
#endif
