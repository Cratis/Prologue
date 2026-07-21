// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Microsoft.Data.SqlClient;

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

/// <summary>
/// Turns Change Data Capture on for the database being watched and the tables in it. The system being captured
/// should not have to know Prologue exists, let alone run setup code for it — so the extractor prepares the
/// database itself rather than expecting the application to do it.
/// </summary>
/// <param name="options">The configuration for this SQL Server source.</param>
/// <param name="logger">The logger.</param>
public class SqlServerChangeCapturePreparer(SqlServerOptions options, ILogger<SqlServerChangeCapturePreparer> logger)
{
    /// <summary>
    /// Narrows the tables found in the database to those the configuration asks for. An empty allowlist means
    /// every table; entries match either the bare table name or a qualified <c>schema.table</c>.
    /// </summary>
    /// <param name="all">The tables found in the database.</param>
    /// <param name="requested">The configured allowlist.</param>
    /// <returns>The tables to enable Change Data Capture on.</returns>
    public static IReadOnlyList<(string Schema, string Table)> Matching(
        IReadOnlyList<(string Schema, string Table)> all,
        IReadOnlyCollection<string> requested)
    {
        if (requested.Count == 0)
        {
            return all;
        }

        var wanted = new HashSet<string>(requested, StringComparer.OrdinalIgnoreCase);

        return [.. all.Where(candidate =>
            wanted.Contains(candidate.Table) ||
            wanted.Contains($"{candidate.Schema}.{candidate.Table}"))];
    }

    /// <summary>
    /// Enables Change Data Capture on the database and on every table that should be watched, skipping whatever
    /// is already enabled. Requires <c>sysadmin</c> and a running SQL Server Agent.
    /// </summary>
    /// <param name="connection">An open connection to the database being watched.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Prepare(SqlConnection connection, CancellationToken cancellationToken)
    {
        if (!options.EnableChangeDataCapture)
        {
            return;
        }

        // Preparing the database is a convenience, not a precondition: a DBA may well have enabled CDC already,
        // and the connecting account may deliberately not be sysadmin. Neither case should stop the extractor —
        // it will simply watch whatever capture instances it finds.
        try
        {
            await EnableForDatabase(connection, cancellationToken);

            var tables = await TablesToEnable(connection, cancellationToken);

            foreach (var (schema, table) in tables)
            {
                await EnableForTable(connection, schema, table, cancellationToken);
            }

            if (tables.Count > 0)
            {
                SqlServerChangeCapturePreparerLog.Enabled(logger, options.Name, tables.Count);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SqlServerChangeCapturePreparerLog.PreparationFailed(logger, options.Name, exception);
        }
    }

    static async Task EnableForDatabase(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            IF (SELECT is_cdc_enabled FROM sys.databases WHERE name = DB_NAME()) = 0
            BEGIN
                EXEC sys.sp_cdc_enable_db;
            END
            """;

        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    static async Task EnableForTable(SqlConnection connection, string schema, string table, CancellationToken cancellationToken)
    {
        const string sql = """
            IF NOT EXISTS (
                SELECT 1
                FROM cdc.change_tables ct
                JOIN sys.tables t ON ct.source_object_id = t.object_id
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = @schema AND t.name = @table)
            BEGIN
                EXEC sys.sp_cdc_enable_table
                    @source_schema = @schema,
                    @source_name = @table,
                    @role_name = NULL,
                    @supports_net_changes = 0;
            END
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue(nameof(schema), schema);
        command.Parameters.AddWithValue(nameof(table), table);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    static async Task<IReadOnlyList<(string Schema, string Table)>> AllUserTables(SqlConnection connection, CancellationToken cancellationToken)
    {
        // Everything the system owns, minus the plumbing CDC itself creates and minus tables without a primary
        // key, which sp_cdc_enable_table rejects.
        const string sql = """
            SELECT s.name, t.name
            FROM sys.tables t
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE t.is_ms_shipped = 0
              AND s.name NOT IN ('cdc', 'sys')
              AND EXISTS (SELECT 1 FROM sys.indexes i WHERE i.object_id = t.object_id AND i.is_primary_key = 1)
            ORDER BY s.name, t.name
            """;

        var tables = new List<(string Schema, string Table)>();

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add((reader.GetString(0), reader.GetString(1)));
        }

        return tables;
    }

    async Task<IReadOnlyList<(string Schema, string Table)>> TablesToEnable(SqlConnection connection, CancellationToken cancellationToken) =>
        Matching(await AllUserTables(connection, cancellationToken), [.. options.Tables]);
}
