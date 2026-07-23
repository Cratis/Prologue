// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the structure of a single table within an observed database schema — its columns and the
/// constraints that govern them. No data values are captured.
/// </summary>
/// <param name="Schema">The schema the table belongs to.</param>
/// <param name="Table">The name of the table.</param>
/// <param name="Columns">The table's columns, in ordinal order.</param>
/// <param name="PrimaryKey">The names of the primary key columns, in key order; empty when the table has no primary key.</param>
/// <param name="ForeignKeys">The foreign-key relationships from this table to others.</param>
/// <param name="UniqueConstraints">The unique constraints and unique indexes on the table, excluding the primary key.</param>
public record SchemaTable(
    string Schema,
    string Table,
    IReadOnlyList<SchemaColumn> Columns,
    IReadOnlyList<string> PrimaryKey,
    IReadOnlyList<SchemaForeignKey> ForeignKeys,
    IReadOnlyList<SchemaUniqueConstraint> UniqueConstraints);
