// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data;
using Microsoft.Data.SqlClient;

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

/// <summary>
/// Reads metadata from SQL Server's native CDC tables — the capture instances, the current max LSN, and the raw
/// change rows within an LSN range. Only metadata columns are selected; row data is never read.
/// </summary>
public class SqlServerCdcReader
{
    /// <summary>
    /// Loads the CDC capture instances and their captured column names from the database.
    /// </summary>
    /// <param name="connection">An open connection to the CDC-enabled database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The capture instances defined in the database.</returns>
    public async Task<IReadOnlyList<CdcCaptureInstance>> LoadInstances(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string instancesSql = """
            SELECT ct.capture_instance, ss.name AS source_schema, st.name AS source_table
            FROM cdc.change_tables ct
            JOIN sys.tables st ON ct.source_object_id = st.object_id
            JOIN sys.schemas ss ON st.schema_id = ss.schema_id
            ORDER BY ct.capture_instance
            """;

        var instances = new List<(string Name, string Schema, string Table)>();
        await using (var command = new SqlCommand(instancesSql, connection))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                instances.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
            }
        }

        var result = new List<CdcCaptureInstance>();
        foreach (var instance in instances)
        {
            var columns = await LoadColumns(connection, instance.Name, cancellationToken);
            result.Add(new CdcCaptureInstance(instance.Name, instance.Schema, instance.Table, columns));
        }

        return result;
    }

    /// <summary>
    /// Gets the current maximum LSN of the database's CDC change tables.
    /// </summary>
    /// <param name="connection">An open connection to the CDC-enabled database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The maximum LSN, or a zero LSN when no changes have been captured yet.</returns>
    public async Task<byte[]> GetMaxLsn(SqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("SELECT sys.fn_cdc_get_max_lsn()", connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result as byte[] ?? new byte[10];
    }

    /// <summary>
    /// Reads the raw change rows committed in the exclusive-inclusive LSN range <paramref name="fromLsn"/> to
    /// <paramref name="toLsn"/> across all capture instances.
    /// </summary>
    /// <param name="connection">An open connection to the CDC-enabled database.</param>
    /// <param name="instances">The capture instances to read from.</param>
    /// <param name="fromLsn">The exclusive lower bound LSN.</param>
    /// <param name="toLsn">The inclusive upper bound LSN.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The raw change rows ordered by LSN.</returns>
    public async Task<IReadOnlyList<CdcRow>> ReadChanges(
        SqlConnection connection,
        IEnumerable<CdcCaptureInstance> instances,
        byte[] fromLsn,
        byte[] toLsn,
        CancellationToken cancellationToken)
    {
        var rows = new List<CdcRow>();
        foreach (var instance in instances)
        {
            var table = $"cdc.[{instance.Name.Replace("]", "]]", StringComparison.Ordinal)}_CT]";
            var sql = $"""
                SELECT [__$start_lsn], [__$operation], [__$update_mask], sys.fn_cdc_map_lsn_to_time([__$start_lsn])
                FROM {table}
                WHERE [__$start_lsn] > @from AND [__$start_lsn] <= @to
                ORDER BY [__$start_lsn], [__$seqval]
                """;

            // The table name is a CDC capture-instance identifier from SQL Server's own system catalog, escaped
            // by doubling ']' — identifiers cannot be parameterized, and the value columns below are.
#pragma warning disable CA2100
            await using var command = new SqlCommand(sql, connection);
#pragma warning restore CA2100
            command.Parameters.Add("@from", SqlDbType.Binary, 10).Value = fromLsn;
            command.Parameters.Add("@to", SqlDbType.Binary, 10).Value = toLsn;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var commitTime = await reader.IsDBNullAsync(3, cancellationToken)
                    ? DateTimeOffset.UtcNow
                    : new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc));

                rows.Add(new CdcRow(
                    (byte[])reader.GetValue(0),
                    commitTime,
                    instance.Schema,
                    instance.Table,
                    reader.GetInt32(1),
                    (byte[])reader.GetValue(2),
                    instance.Columns));
            }
        }

        return rows;
    }

    static async Task<IReadOnlyList<string>> LoadColumns(SqlConnection connection, string instance, CancellationToken cancellationToken)
    {
        const string columnsSql = """
            SELECT cc.column_name
            FROM cdc.captured_columns cc
            JOIN cdc.change_tables ct ON cc.object_id = ct.object_id
            WHERE ct.capture_instance = @instance
            ORDER BY cc.column_ordinal
            """;

        await using var command = new SqlCommand(columnsSql, connection);
        command.Parameters.AddWithValue("@instance", instance);

        var columns = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }
}
