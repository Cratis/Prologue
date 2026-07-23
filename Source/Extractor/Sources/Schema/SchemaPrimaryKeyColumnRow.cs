// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.Schema;

/// <summary>
/// Represents a single raw primary key column row read from a database's catalog — one column of one table's
/// primary key. Rows arrive in key order.
/// </summary>
/// <param name="Schema">The schema of the table the key belongs to.</param>
/// <param name="Table">The name of the table the key belongs to.</param>
/// <param name="Column">The name of the key column.</param>
public record SchemaPrimaryKeyColumnRow(string Schema, string Table, string Column);
