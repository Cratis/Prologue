// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.Schema;

/// <summary>
/// Assembles the raw catalog rows a schema reader produced into a single <see cref="DatabaseSchemaObserved"/>
/// observation — grouping columns per table and attaching each table's primary key, foreign keys, and unique
/// constraints. This is pure metadata assembly — no data values are involved.
/// </summary>
public static class SchemaObservationBuilder
{
    /// <summary>
    /// Builds the schema observation for a database from the raw rows read from its catalog.
    /// </summary>
    /// <param name="source">The kind of source the schema was read from.</param>
    /// <param name="sourceName">The configured logical name of the source.</param>
    /// <param name="database">The name of the database the schema belongs to.</param>
    /// <param name="observed">The moment the schema was read.</param>
    /// <param name="rows">The raw rows read from the database's catalog.</param>
    /// <returns>An <see cref="Observation"/> carrying the <see cref="DatabaseSchemaObserved"/> payload.</returns>
    public static Observation Build(
        SourceKind source,
        string sourceName,
        string database,
        DateTimeOffset observed,
        SchemaRows rows)
    {
        var tables = rows.Columns
            .GroupBy(row => (row.Schema, row.Table))
            .Select(group => new SchemaTable(
                group.Key.Schema,
                group.Key.Table,
                [.. group.Select(row => new SchemaColumn(row.Name, row.DataType, row.MaxLength, row.IsNullable, row.IsIdentity, row.DefaultExpression))],
                PrimaryKeyOf(rows, group.Key),
                ForeignKeysOf(rows, group.Key),
                UniqueConstraintsOf(rows, group.Key)))
            .ToList();

        var payload = new DatabaseSchemaObserved(source.Value, database, sourceName, tables);
        return new Observation(source, observed, payload);
    }

    static IReadOnlyList<string> PrimaryKeyOf(SchemaRows rows, (string Schema, string Table) table) =>
        [.. rows.PrimaryKeys
            .Where(row => (row.Schema, row.Table) == table)
            .Select(row => row.Column)];

    static IReadOnlyList<SchemaForeignKey> ForeignKeysOf(SchemaRows rows, (string Schema, string Table) table) =>
        [.. rows.ForeignKeys
            .Where(row => (row.Schema, row.Table) == table)
            .GroupBy(row => row.Name)
            .Select(group => new SchemaForeignKey(
                group.Key,
                [.. group.Select(row => row.Column)],
                group.First().ReferencedSchema,
                group.First().ReferencedTable,
                [.. group.Select(row => row.ReferencedColumn)]))];

    static IReadOnlyList<SchemaUniqueConstraint> UniqueConstraintsOf(SchemaRows rows, (string Schema, string Table) table) =>
        [.. rows.UniqueConstraints
            .Where(row => (row.Schema, row.Table) == table)
            .GroupBy(row => row.Name)
            .Select(group => new SchemaUniqueConstraint(group.Key, [.. group.Select(row => row.Column)]))];
}
