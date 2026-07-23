// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Extractor.Sources.Schema;
using Npgsql;

namespace Cratis.Prologue.Extractor.Sources.Postgres;

/// <summary>
/// Reads the structure of a PostgreSQL database from <c>information_schema</c> and <c>pg_catalog</c> — columns,
/// primary keys, foreign keys, and unique constraints — as the flat rows
/// <see cref="SchemaObservationBuilder"/> assembles into an observation. Only metadata is selected; no data
/// values are ever read.
/// </summary>
public class PostgresSchemaReader
{
    const string ColumnsSql = """
        SELECT c.table_schema, c.table_name, c.column_name, c.data_type,
               COALESCE(c.character_maximum_length, 0),
               c.is_nullable = 'YES',
               c.is_identity = 'YES' OR COALESCE(c.column_default, '') LIKE 'nextval(%',
               COALESCE(c.column_default, '')
        FROM information_schema.columns c
        JOIN information_schema.tables t
          ON t.table_schema = c.table_schema AND t.table_name = c.table_name AND t.table_type = 'BASE TABLE'
        WHERE c.table_schema NOT IN ('pg_catalog', 'information_schema')
        ORDER BY c.table_schema, c.table_name, c.ordinal_position
        """;

    const string PrimaryKeysSql = """
        SELECT tc.table_schema, tc.table_name, kcu.column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
          ON kcu.constraint_schema = tc.constraint_schema AND kcu.constraint_name = tc.constraint_name
        WHERE tc.constraint_type = 'PRIMARY KEY'
          AND tc.table_schema NOT IN ('pg_catalog', 'information_schema')
        ORDER BY tc.table_schema, tc.table_name, kcu.ordinal_position
        """;

    // information_schema cannot express the column pairing of a multi-column foreign key, so the pairs come from
    // pg_catalog by unnesting the constraint's referencing and referenced column arrays together.
    const string ForeignKeysSql = """
        SELECT ns.nspname, tbl.relname, con.conname, att.attname, fns.nspname, ftbl.relname, fatt.attname
        FROM pg_constraint con
        JOIN pg_class tbl ON tbl.oid = con.conrelid
        JOIN pg_namespace ns ON ns.oid = tbl.relnamespace
        JOIN pg_class ftbl ON ftbl.oid = con.confrelid
        JOIN pg_namespace fns ON fns.oid = ftbl.relnamespace
        JOIN LATERAL unnest(con.conkey, con.confkey) WITH ORDINALITY AS cols(attnum, fattnum, ordinal) ON true
        JOIN pg_attribute att ON att.attrelid = con.conrelid AND att.attnum = cols.attnum
        JOIN pg_attribute fatt ON fatt.attrelid = con.confrelid AND fatt.attnum = cols.fattnum
        WHERE con.contype = 'f' AND ns.nspname NOT IN ('pg_catalog', 'information_schema')
        ORDER BY ns.nspname, tbl.relname, con.conname, cols.ordinal
        """;

    const string UniqueConstraintsSql = """
        SELECT tc.table_schema, tc.table_name, tc.constraint_name, kcu.column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
          ON kcu.constraint_schema = tc.constraint_schema AND kcu.constraint_name = tc.constraint_name
        WHERE tc.constraint_type = 'UNIQUE'
          AND tc.table_schema NOT IN ('pg_catalog', 'information_schema')
        ORDER BY tc.table_schema, tc.table_name, tc.constraint_name, kcu.ordinal_position
        """;

    /// <summary>
    /// Reads the full schema of the connected database as flat rows.
    /// </summary>
    /// <param name="connection">An open connection to the database being watched.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The <see cref="SchemaRows"/> read from the catalog.</returns>
    public async Task<SchemaRows> Read(NpgsqlConnection connection, CancellationToken cancellationToken) =>
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
        NpgsqlConnection connection,
        string sql,
        Func<NpgsqlDataReader, TRow> materialize,
        CancellationToken cancellationToken)
    {
        var rows = new List<TRow>();

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(materialize(reader));
        }

        return rows;
    }
#pragma warning restore CA2100
}
