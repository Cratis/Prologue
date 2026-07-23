// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Extractor.Sources.Schema;
using Microsoft.Data.SqlClient;

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

/// <summary>
/// Reads the structure of a SQL Server database from its system catalog — columns, primary keys, foreign keys,
/// and unique constraints — as the flat rows <see cref="SchemaObservationBuilder"/> assembles into an
/// observation. Only metadata is selected; no data values are ever read.
/// </summary>
public class SqlServerSchemaReader
{
    const string ColumnsSql = """
        SELECT s.name, t.name, c.name, ty.name,
               CAST(CASE
                   WHEN ty.name NOT IN (N'char', N'varchar', N'nchar', N'nvarchar', N'binary', N'varbinary') THEN 0
                   WHEN c.max_length = -1 THEN 0
                   WHEN ty.name IN (N'nchar', N'nvarchar') THEN c.max_length / 2
                   ELSE c.max_length
               END AS int),
               c.is_nullable, c.is_identity, ISNULL(dc.definition, N'')
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.schemas s ON t.schema_id = s.schema_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
        WHERE t.is_ms_shipped = 0 AND s.name NOT IN (N'cdc', N'sys')
        ORDER BY s.name, t.name, c.column_id
        """;

    const string PrimaryKeysSql = """
        SELECT s.name, t.name, c.name
        FROM sys.indexes i
        JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        JOIN sys.tables t ON i.object_id = t.object_id
        JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE i.is_primary_key = 1 AND t.is_ms_shipped = 0 AND s.name NOT IN (N'cdc', N'sys')
        ORDER BY s.name, t.name, ic.key_ordinal
        """;

    const string ForeignKeysSql = """
        SELECT s.name, t.name, fk.name, pc.name, rs.name, rt.name, rc.name
        FROM sys.foreign_keys fk
        JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        JOIN sys.tables t ON fk.parent_object_id = t.object_id
        JOIN sys.schemas s ON t.schema_id = s.schema_id
        JOIN sys.columns pc ON fkc.parent_object_id = pc.object_id AND fkc.parent_column_id = pc.column_id
        JOIN sys.tables rt ON fk.referenced_object_id = rt.object_id
        JOIN sys.schemas rs ON rt.schema_id = rs.schema_id
        JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
        WHERE t.is_ms_shipped = 0 AND s.name NOT IN (N'cdc', N'sys')
        ORDER BY s.name, t.name, fk.name, fkc.constraint_column_id
        """;

    const string UniqueConstraintsSql = """
        SELECT s.name, t.name, i.name, c.name
        FROM sys.indexes i
        JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        JOIN sys.tables t ON i.object_id = t.object_id
        JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE i.is_unique = 1 AND i.is_primary_key = 0 AND i.has_filter = 0
          AND t.is_ms_shipped = 0 AND s.name NOT IN (N'cdc', N'sys')
        ORDER BY s.name, t.name, i.name, ic.key_ordinal
        """;

    /// <summary>
    /// Reads the full schema of the connected database as flat rows.
    /// </summary>
    /// <param name="connection">An open connection to the database being watched.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The <see cref="SchemaRows"/> read from the catalog.</returns>
    public async Task<SchemaRows> Read(SqlConnection connection, CancellationToken cancellationToken) =>
        new(
            await Rows(
                connection,
                ColumnsSql,
                reader => new SchemaColumnRow(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetBoolean(5),
                    reader.GetBoolean(6),
                    reader.GetString(7)),
                cancellationToken),
            await Rows(
                connection,
                PrimaryKeysSql,
                reader => new SchemaPrimaryKeyColumnRow(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2)),
                cancellationToken),
            await Rows(
                connection,
                ForeignKeysSql,
                reader => new SchemaForeignKeyColumnRow(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetString(6)),
                cancellationToken),
            await Rows(
                connection,
                UniqueConstraintsSql,
                reader => new SchemaUniqueConstraintColumnRow(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3)),
                cancellationToken));

    // sql is only ever one of the const catalog queries above — never externally supplied.
#pragma warning disable CA2100
    static async Task<IReadOnlyList<TRow>> Rows<TRow>(
        SqlConnection connection,
        string sql,
        Func<SqlDataReader, TRow> materialize,
        CancellationToken cancellationToken)
    {
        var rows = new List<TRow>();

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(materialize(reader));
        }

        return rows;
    }
#pragma warning restore CA2100
}
