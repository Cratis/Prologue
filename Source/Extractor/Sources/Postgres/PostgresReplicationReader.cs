// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Extractor.Capturing;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;

namespace Cratis.Prologue.Extractor.Sources.Postgres;

/// <summary>
/// Streams metadata about committed transactions from a PostgreSQL database using logical replication
/// (the <c>pgoutput</c> plugin), grouping each transaction's table changes between its <c>BEGIN</c> and <c>COMMIT</c>.
/// Only table and column names are extracted; no row values are read.
/// </summary>
public class PostgresReplicationReader
{
    /// <summary>
    /// Ensures the publication and logical replication slot the reader consumes both exist, creating them when absent.
    /// </summary>
    /// <param name="options">The configuration for the PostgreSQL source.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task EnsureInfrastructure(PostgresOptions options, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        if (!await Exists(connection, "SELECT 1 FROM pg_publication WHERE pubname = @name", options.Publication, cancellationToken))
        {
            var publication = options.Publication.Replace("\"", "\"\"", StringComparison.Ordinal);
            await Execute(connection, $"CREATE PUBLICATION \"{publication}\" FOR ALL TABLES", cancellationToken);
        }

        if (!await Exists(connection, "SELECT 1 FROM pg_replication_slots WHERE slot_name = @name", options.Slot, cancellationToken))
        {
            await using var command = new NpgsqlCommand("SELECT pg_create_logical_replication_slot(@name, 'pgoutput')", connection);
            command.Parameters.AddWithValue("name", options.Slot);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Streams one observation per committed transaction that changed at least one table.
    /// </summary>
    /// <param name="options">The configuration for the PostgreSQL source.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that stops the stream.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of per-transaction observations.</returns>
    public async IAsyncEnumerable<Observation> Stream(
        PostgresOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var database = new NpgsqlConnectionStringBuilder(options.ConnectionString).Database ?? string.Empty;
        await using var connection = new LogicalReplicationConnection(options.ConnectionString);
        await connection.Open(cancellationToken);

        var slot = new PgOutputReplicationSlot(options.Slot);
        var replicationOptions = new PgOutputReplicationOptions(options.Publication, PgOutputProtocolVersion.V1);
        var accumulator = new PgTransactionAccumulator();

        await foreach (var message in connection.StartReplication(slot, replicationOptions, cancellationToken))
        {
            var observation = await Handle(message, accumulator, database, cancellationToken);
            if (observation is not null)
            {
                yield return observation;
            }

            connection.SetReplicationStatus(message.WalEnd);
        }
    }

    static async Task<Observation?> Handle(
        PgOutputReplicationMessage message,
        PgTransactionAccumulator accumulator,
        string database,
        CancellationToken cancellationToken)
    {
        switch (message)
        {
            case BeginMessage begin:
                accumulator.Begin(begin.TransactionXid.ToString());
                return null;
            case InsertMessage insert:
                await Drain(insert.NewRow, cancellationToken);
                Record(accumulator, insert.Relation, ChangeOperation.Insert);
                return null;
            case UpdateMessage update:
                await Drain(update.NewRow, cancellationToken);
                Record(accumulator, update.Relation, ChangeOperation.Update);
                return null;
            case DeleteMessage delete:
                Record(accumulator, delete.Relation, ChangeOperation.Delete);
                return null;
            case CommitMessage commit:
                var committed = new DateTimeOffset(DateTime.SpecifyKind(commit.TransactionCommitTimestamp, DateTimeKind.Utc));
                return accumulator.Complete(database, committed);
            default:
                return null;
        }
    }

    static void Record(PgTransactionAccumulator accumulator, RelationMessage relation, ChangeOperation operation)
    {
        var columns = relation.Columns.Select(column => column.ColumnName).ToList();
        accumulator.Record(relation.Namespace, relation.RelationName, operation, columns);
    }

    static async Task Drain(ReplicationTuple tuple, CancellationToken cancellationToken)
    {
#pragma warning disable SA1312
        await foreach (var _ in tuple.WithCancellation(cancellationToken))
        {
        }
#pragma warning restore SA1312
    }

    // sql is only ever passed by the two call sites above: a literal with a parameterized value, or an
    // identifier interpolated after quote-escaping - never externally supplied.
#pragma warning disable CA2100
    static async Task<bool> Exists(NpgsqlConnection connection, string sql, string name, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue(nameof(name), name);
        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    static async Task Execute(NpgsqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
#pragma warning restore CA2100
}
