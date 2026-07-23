// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.Schema;

/// <summary>
/// Represents a single raw unique constraint column row read from a database's catalog — one column of one
/// unique constraint or unique index. Rows for a constraint arrive in constraint column order.
/// </summary>
/// <param name="Schema">The schema of the table the constraint belongs to.</param>
/// <param name="Table">The name of the table the constraint belongs to.</param>
/// <param name="Name">The name of the constraint or index.</param>
/// <param name="Column">The name of the constrained column.</param>
public record SchemaUniqueConstraintColumnRow(string Schema, string Table, string Name, string Column);
