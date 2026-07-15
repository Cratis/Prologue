// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.Postgres;

/// <summary>
/// Accumulates the table changes streamed between a PostgreSQL logical-replication <c>BEGIN</c> and <c>COMMIT</c>
/// into a single per-transaction observation. This is pure metadata collection — no row values are recorded.
/// </summary>
public class PgTransactionAccumulator
{
    readonly List<TableChange> _changes = [];
    string _transactionId = string.Empty;

    /// <summary>
    /// Begins a new transaction, discarding any partially accumulated state.
    /// </summary>
    /// <param name="transactionId">The transaction id reported by the <c>BEGIN</c> message.</param>
    public void Begin(string transactionId)
    {
        _changes.Clear();
        _transactionId = transactionId;
    }

    /// <summary>
    /// Records a change to a table within the current transaction.
    /// </summary>
    /// <param name="schema">The schema of the changed table.</param>
    /// <param name="table">The name of the changed table.</param>
    /// <param name="operation">The kind of change.</param>
    /// <param name="columns">The names of the table's columns.</param>
    public void Record(string schema, string table, ChangeOperation operation, IReadOnlyList<string> columns) =>
        _changes.Add(new TableChange(schema, table, operation, columns));

    /// <summary>
    /// Completes the current transaction, producing an observation when it changed at least one table.
    /// </summary>
    /// <param name="database">The name of the database the transaction occurred in.</param>
    /// <param name="committed">The moment the transaction committed.</param>
    /// <returns>An <see cref="Observation"/> for the transaction, or <see langword="null"/> when no tables changed.</returns>
    public Observation? Complete(string database, DateTimeOffset committed)
    {
        if (_changes.Count == 0)
        {
            return null;
        }

        var payload = new DatabaseTransactionObserved(SourceKind.Postgres.Value, database, _transactionId, [.. _changes]);
        _changes.Clear();
        return new Observation(SourceKind.Postgres, committed, payload);
    }
}
