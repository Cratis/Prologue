// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Infers the uniqueness constraints for a slice from the unique constraints the observed schema declares on the
/// tables its correlated transactions wrote. A single-column unique constraint whose column the produced event
/// covers becomes an <see cref="ExtractedConstraint"/> on the slice — evidence the captured system treats that
/// property as an identity. Multi-column constraints stay evidence-only; they do not map to a single-property
/// constraint.
/// </summary>
public static class ConstraintInference
{
    /// <summary>
    /// Infers the uniqueness constraints evidenced by the tables the transactions wrote.
    /// </summary>
    /// <param name="transactions">The database transactions correlated with the slice's command.</param>
    /// <param name="schema">The observed schema to source unique constraints from.</param>
    /// <returns>The inferred constraints.</returns>
    public static IReadOnlyList<ExtractedConstraint> ForSlice(
        IReadOnlyList<DatabaseTransactionObserved> transactions,
        SchemaIndex schema) =>
    [
        .. transactions
            .SelectMany(transaction => transaction.Tables.Select(table => (transaction.Database, Table: table)))
            .Select(scoped => (scoped.Table, SchemaTable: schema.Find(scoped.Database, scoped.Table.Schema, scoped.Table.Table)))
            .Where(scoped => scoped.SchemaTable is not null)
            .SelectMany(scoped => ForTable(scoped.Table, scoped.SchemaTable!))
            .GroupBy(constraint => constraint.Name)
            .Select(group => group.First())
    ];

    static IEnumerable<ExtractedConstraint> ForTable(TableChange table, SchemaTable schemaTable)
    {
        var entity = Naming.Singularize(Naming.Pascalize(table.Table));
        var onEvent = Naming.EventName(entity, table.Operation);
        return schemaTable.UniqueConstraints
            .Where(constraint => constraint.Columns.Count == 1 &&
                table.Columns.Contains(constraint.Columns[0], StringComparer.OrdinalIgnoreCase))
            .Select(constraint => new ExtractedConstraint(
                $"Unique{Naming.Pascalize(constraint.Columns[0])}",
                Naming.Camelize(constraint.Columns[0]),
                onEvent));
    }
}
