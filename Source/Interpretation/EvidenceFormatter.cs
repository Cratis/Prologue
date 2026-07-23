// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Cratis.Prologue.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Formats a Prologue's captures into the compact evidence text the refinement prompt gives the language model —
/// the distinct observed behavior (HTTP commands, table changes, telemetry spans) and, when a database schema was
/// captured, one line per table with its column types, sizes, nullability, keys, unique constraints, and
/// relationships. Every category is capped so a large system never blows the prompt budget.
/// </summary>
public static class EvidenceFormatter
{
    const int MaxEntries = 50;

    /// <summary>
    /// Formats the distinct observed behavior across the captures, one line per observation.
    /// </summary>
    /// <param name="captures">The captures to format the observed behavior of.</param>
    /// <returns>The observed-behavior lines.</returns>
    public static string ObservedBehavior(IReadOnlyList<Capture> captures)
    {
        var payloads = captures.SelectMany(capture => capture.Entries).Select(entry => entry.Payload).ToList();
        var builder = new StringBuilder();

        foreach (var http in payloads.OfType<HttpCommandObserved>().Select(http => $"{http.Method} {http.Path}").Distinct().Take(MaxEntries))
        {
            builder.AppendLine($"- HTTP: {http}");
        }

        foreach (var table in payloads.OfType<DatabaseTransactionObserved>().SelectMany(transaction => transaction.Tables)
            .Select(table => $"{table.Table} [{table.Operation}]: {string.Join(", ", table.Columns)}").Distinct().Take(MaxEntries))
        {
            builder.AppendLine($"- DB table {table}");
        }

        foreach (var span in payloads.OfType<TelemetryObserved>().Select(span => $"{span.ServiceName}: {span.Name}").Distinct().Take(MaxEntries))
        {
            builder.AppendLine($"- Span: {span}");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Formats the observed database schema across the captures, one line per table — empty when no
    /// <see cref="DatabaseSchemaObserved"/> capture is present.
    /// </summary>
    /// <param name="captures">The captures to format the observed schema of.</param>
    /// <returns>The schema lines, or an empty string when no schema was observed.</returns>
    public static string DatabaseSchema(IReadOnlyList<Capture> captures)
    {
        var schemas = captures
            .SelectMany(capture => capture.Entries)
            .Select(entry => entry.Payload)
            .OfType<DatabaseSchemaObserved>()
            .ToList();
        if (schemas.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var schema in schemas)
        {
            foreach (var table in schema.Tables.Take(MaxEntries))
            {
                builder.AppendLine($"- {schema.Database} {table.Schema}.{table.Table}: {Columns(table)}{Uniques(table)}{ForeignKeys(table)}");
            }
        }

        return builder.ToString();
    }

    static string Columns(SchemaTable table) => string.Join(", ", table.Columns.Select(column => Column(table, column)));

    static string Column(SchemaTable table, SchemaColumn column) =>
        $"{column.Name} {column.DataType}" +
        (column.MaxLength > 0 ? $"({column.MaxLength})" : string.Empty) +
        (table.IsPrimaryKey(column.Name) ? " pk" : string.Empty) +
        (column.IsIdentity ? " identity" : string.Empty) +
        (column.IsNullable ? string.Empty : " required");

    static string Uniques(SchemaTable table) =>
        table.UniqueConstraints.Count == 0
            ? string.Empty
            : string.Concat(table.UniqueConstraints.Select(unique => $"; unique({string.Join(", ", unique.Columns)})"));

    static string ForeignKeys(SchemaTable table) =>
        table.ForeignKeys.Count == 0
            ? string.Empty
            : string.Concat(table.ForeignKeys.Select(foreignKey =>
                $"; fk ({string.Join(", ", foreignKey.Columns)}) -> {foreignKey.ReferencedSchema}.{foreignKey.ReferencedTable}({string.Join(", ", foreignKey.ReferencedColumns)})"));
}
