// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Infers the properties of commands, events, and read models from the columns observed changing in database
/// transactions and the allowlisted attribute keys observed on telemetry spans. No data values are ever seen —
/// only column and attribute names, plus the observed schema when one was captured: a schema column decides a
/// property's type, requiredness, and maximum length, and name conventions fill in when no schema covers it.
/// </summary>
public static class PropertyInference
{
    /// <summary>
    /// Infers the input properties of a command from the columns its correlated transactions changed and the
    /// attribute keys its spans carried.
    /// </summary>
    /// <param name="transactions">The database transactions correlated with the command.</param>
    /// <param name="spans">The telemetry spans correlated with the command.</param>
    /// <param name="schema">The observed schema to source types and constraints from.</param>
    /// <returns>The inferred command properties.</returns>
    public static IReadOnlyList<ExtractedProperty> ForCommand(
        IReadOnlyList<DatabaseTransactionObserved> transactions,
        IReadOnlyList<TelemetryObserved> spans,
        SchemaIndex schema)
    {
        var columns = ColumnsOf(transactions, schema, _ => true);
        var attributes = spans
            .SelectMany(span => span.AttributeKeys)
            .Select(key => (Name: key, Column: default(SchemaColumn)));
        return Distinct(columns.Concat(attributes));
    }

    /// <summary>
    /// Infers the properties of a read model for a specific entity from the columns changed on the tables that map
    /// to that entity.
    /// </summary>
    /// <param name="transactions">The database transactions the entity's read model is built from.</param>
    /// <param name="entity">The singular, PascalCase entity name.</param>
    /// <param name="schema">The observed schema to source types and constraints from.</param>
    /// <returns>The inferred read-model properties.</returns>
    public static IReadOnlyList<ExtractedProperty> ForEntity(
        IReadOnlyList<DatabaseTransactionObserved> transactions,
        string entity,
        SchemaIndex schema) =>
        Distinct(ColumnsOf(transactions, schema, table => Naming.Singularize(Naming.Pascalize(table.Table)) == entity));

    /// <summary>
    /// Builds a property from a raw column or attribute name and the schema column covering it, when one was
    /// observed. The schema decides the type, requiredness, and maximum length; name conventions fill in without one.
    /// </summary>
    /// <param name="name">The raw column or attribute name.</param>
    /// <param name="column">The schema column covering the name; <see langword="null"/> when none was observed.</param>
    /// <returns>The <see cref="ExtractedProperty"/>.</returns>
    public static ExtractedProperty Property(string name, SchemaColumn? column)
    {
        var pascalized = Naming.Pascalize(name);
        var type = (column is not null ? SchemaTypeMapping.TypeFor(column.DataType) : null) ?? Naming.InferType(pascalized);
        return new ExtractedProperty(
            pascalized,
            type,
            column is { IsNullable: false },
            column is not null && type == "string" ? column.MaxLength : 0);
    }

    static IEnumerable<(string Name, SchemaColumn? Column)> ColumnsOf(
        IReadOnlyList<DatabaseTransactionObserved> transactions,
        SchemaIndex schema,
        Func<TableChange, bool> tables) =>
        transactions
            .SelectMany(transaction => transaction.Tables.Where(tables).Select(table => (transaction.Database, Table: table)))
            .SelectMany(scoped =>
            {
                var schemaTable = schema.Find(scoped.Database, scoped.Table.Schema, scoped.Table.Table);
                return scoped.Table.Columns.Select(column => (Name: column, Column: schemaTable?.Column(column)));
            });

    static IReadOnlyList<ExtractedProperty> Distinct(IEnumerable<(string Name, SchemaColumn? Column)> candidates) =>
    [
        .. candidates
            .GroupBy(candidate => Naming.Pascalize(candidate.Name))
            .Select(group => Property(
                group.Key,
                group.Select(candidate => candidate.Column).FirstOrDefault(column => column is not null)))
    ];
}
