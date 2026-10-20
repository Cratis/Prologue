// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Infers events from transaction metadata, treating an ORM's insert followed by updates to the same table in
/// one transaction as creation. Metadata contains no row identities, so this is a provisional table-level inference.
/// </summary>
internal static class EventInference
{
    internal static IReadOnlyList<ExtractedEvent> From(IReadOnlyList<DatabaseTransactionObserved> transactions, SchemaIndex schema) =>
    [
        .. transactions.SelectMany(transaction => ChangesIn(transaction).Select(table =>
        {
            var schemaTable = schema.Find(transaction.Database, table.Schema, table.Table);
            var entity = Naming.Singularize(Naming.Pascalize(table.Table));
            return new ExtractedEvent(
                Naming.EventName(entity, table.Operation),
                [.. table.Columns.Select(column => PropertyInference.Property(column, schemaTable?.Column(column)))]);
        }))
    ];

    static IEnumerable<TableChange> ChangesIn(DatabaseTransactionObserved transaction) =>
        transaction.Tables
            .GroupBy(table => (table.Schema, table.Table))
            .SelectMany(group => group
                .GroupBy(table => table.Operation == ChangeOperation.Update && group.First().Operation == ChangeOperation.Insert
                    ? ChangeOperation.Insert
                    : table.Operation)
                .Select(changes => new TableChange(
                    group.Key.Schema,
                    group.Key.Table,
                    changes.Key,
                    [.. changes.SelectMany(table => table.Columns).Distinct(StringComparer.Ordinal)])));
}
