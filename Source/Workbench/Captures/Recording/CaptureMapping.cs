// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Captures.Recording;

/// <summary>
/// Maps a capture the Extractor sent to the command that records it.
/// </summary>
public static class CaptureMapping
{
    /// <summary>
    /// The most spans a recording keeps; the count of all of them is kept regardless.
    /// </summary>
    public const int MaximumSpans = 25;

    /// <summary>
    /// Maps a capture to the command recording it. The first HTTP command is the capture's command; schema, metric and log
    /// observations are not part of the record.
    /// </summary>
    /// <param name="capture">The capture.</param>
    /// <returns>The command, for the capture's prologue or the unassigned one.</returns>
    public static RecordCapture ToCommand(Capture capture)
    {
        var start = capture.Entries.Count == 0 ? capture.Occurred : capture.Entries.Min(entry => entry.Occurred);
        int Offset(DateTimeOffset at) => (int)Math.Max(0, (at - start).TotalMilliseconds);

        var command = capture.Entries.Where(entry => entry.Payload is HttpCommandObserved).Select(entry => (entry.Occurred, Payload: (HttpCommandObserved)entry.Payload)).FirstOrDefault();
        var http = command.Payload is null ? RecordedCommand.None : ToRecorded(command.Payload, Offset(command.Occurred));

        var transactions = capture.Entries
            .Where(entry => entry.Payload is DatabaseTransactionObserved)
            .Select(entry => new RecordedTransaction(ToChanges((DatabaseTransactionObserved)entry.Payload), Offset(entry.Occurred)))
            .ToList();

        var spans = capture.Entries
            .Where(entry => entry.Payload is TelemetryObserved)
            .Select(entry => (Observation: entry, Payload: (TelemetryObserved)entry.Payload))
            .ToList();

        var traceLinked = http.Trace != TraceId.None && spans.Exists(span => span.Payload.TraceId == http.Trace.Value);
        var prologue = capture.PrologueId == Guid.Empty ? PrologueId.Unassigned : new PrologueId(capture.PrologueId);

        return new RecordCapture(
            prologue,
            capture.Id,
            capture.Occurred,
            capture.Service,
            http,
            transactions,
            spans.Count,
            [.. spans.Take(MaximumSpans).Select(span => new RecordedSpan(span.Payload.Name, span.Payload.ServiceName, Offset(span.Observation.Occurred), (int)Math.Min(int.MaxValue, span.Payload.DurationMilliseconds)))],
            traceLinked);
    }

    static RecordedCommand ToRecorded(HttpCommandObserved command, int offset) =>
        new(command.Method.ToUpperInvariant(), PathWithoutQuery(command.Path), command.StatusCode, command.TraceId, CommandShape.From(command.Method, command.Path), offset);

    static string PathWithoutQuery(string path) => path.Split('?', '#')[0];

    static RecordedTableChange[] ToChanges(DatabaseTransactionObserved transaction) =>
        [.. transaction.Tables.Select(change => new RecordedTableChange(change.Schema, new TableName(change.Table), change.Operation, [.. change.Columns.Select(column => new ColumnName(column))]))];
}
