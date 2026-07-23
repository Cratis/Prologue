// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Indexes the <see cref="DatabaseSchemaObserved"/> observations found in a Prologue's captures so the heuristics
/// can join the observed schema to the database transactions it governs — by database name plus schema-qualified
/// table name, which is the identity both sides of the join share. When no schema was captured the index is
/// empty and every lookup misses, leaving the name-convention heuristics in charge.
/// </summary>
public class SchemaIndex
{
    readonly Dictionary<string, SchemaTable> _tables;

    SchemaIndex(Dictionary<string, SchemaTable> tables) => _tables = tables;

    /// <summary>
    /// Builds a schema index from the <see cref="DatabaseSchemaObserved"/> observations in the given captures.
    /// </summary>
    /// <param name="captures">The captures to index the schema observations of.</param>
    /// <returns>A <see cref="SchemaIndex"/> covering every observed table.</returns>
    public static SchemaIndex From(IReadOnlyList<Capture> captures)
    {
        var tables = new Dictionary<string, SchemaTable>(StringComparer.OrdinalIgnoreCase);
        var schemas = captures
            .SelectMany(capture => capture.Entries)
            .Select(entry => entry.Payload)
            .OfType<DatabaseSchemaObserved>();

        foreach (var schema in schemas)
        {
            foreach (var table in schema.Tables)
            {
                tables[KeyFor(schema.Database, table.Schema, table.Table)] = table;
            }
        }

        return new SchemaIndex(tables);
    }

    /// <summary>
    /// Finds the observed schema for a table a transaction changed.
    /// </summary>
    /// <param name="database">The database the transaction occurred in.</param>
    /// <param name="schema">The schema the changed table belongs to.</param>
    /// <param name="table">The name of the changed table.</param>
    /// <returns>The <see cref="SchemaTable"/>, or <see langword="null"/> when the table was not observed.</returns>
    public SchemaTable? Find(string database, string schema, string table) =>
        _tables.GetValueOrDefault(KeyFor(database, schema, table));

    static string KeyFor(string database, string schema, string table) => $"{database}|{schema}|{table}";
}
