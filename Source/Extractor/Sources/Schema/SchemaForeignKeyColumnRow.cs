// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.Schema;

/// <summary>
/// Represents a single raw foreign key column row read from a database's catalog — one referencing/referenced
/// column pair of one foreign-key constraint. Rows for a constraint arrive in constraint column order.
/// </summary>
/// <param name="Schema">The schema of the referencing table.</param>
/// <param name="Table">The name of the referencing table.</param>
/// <param name="Name">The name of the foreign-key constraint.</param>
/// <param name="Column">The name of the referencing column.</param>
/// <param name="ReferencedSchema">The schema of the referenced table.</param>
/// <param name="ReferencedTable">The name of the referenced table.</param>
/// <param name="ReferencedColumn">The name of the referenced column.</param>
public record SchemaForeignKeyColumnRow(
    string Schema,
    string Table,
    string Name,
    string Column,
    string ReferencedSchema,
    string ReferencedTable,
    string ReferencedColumn);
