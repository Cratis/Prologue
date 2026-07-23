// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.Schema;

/// <summary>
/// Represents everything a schema reader read from a database's catalog — the raw column, primary key, foreign
/// key, and unique constraint rows <see cref="SchemaObservationBuilder"/> assembles into an observation. This is
/// pure metadata; no data values are ever read.
/// </summary>
/// <param name="Columns">The column rows, in table and ordinal order.</param>
/// <param name="PrimaryKeys">The primary key column rows, in table and key order.</param>
/// <param name="ForeignKeys">The foreign key column rows, in table, constraint, and constraint column order.</param>
/// <param name="UniqueConstraints">The unique constraint column rows, in table, constraint, and constraint column order.</param>
public record SchemaRows(
    IReadOnlyList<SchemaColumnRow> Columns,
    IReadOnlyList<SchemaPrimaryKeyColumnRow> PrimaryKeys,
    IReadOnlyList<SchemaForeignKeyColumnRow> ForeignKeys,
    IReadOnlyList<SchemaUniqueConstraintColumnRow> UniqueConstraints)
{
    /// <summary>
    /// Gets the distinct tables the column rows cover, in the order they first appear.
    /// </summary>
    public IReadOnlyList<(string Schema, string Table)> Tables =>
        [.. Columns.Select(row => (row.Schema, row.Table)).Distinct()];

    /// <summary>
    /// Narrows the rows to those belonging to the given tables — how a source honors its configured table
    /// allowlist before the rows become an observation.
    /// </summary>
    /// <param name="tables">The tables to keep.</param>
    /// <returns>A new <see cref="SchemaRows"/> holding only rows for the given tables.</returns>
    public SchemaRows OnlyTables(IEnumerable<(string Schema, string Table)> tables)
    {
        var allowed = new HashSet<(string Schema, string Table)>(tables);

        return new(
            [.. Columns.Where(row => allowed.Contains((row.Schema, row.Table)))],
            [.. PrimaryKeys.Where(row => allowed.Contains((row.Schema, row.Table)))],
            [.. ForeignKeys.Where(row => allowed.Contains((row.Schema, row.Table)))],
            [.. UniqueConstraints.Where(row => allowed.Contains((row.Schema, row.Table)))]);
    }
}
