// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Lookup helpers for consuming an observed <see cref="SchemaTable"/> in the heuristics — resolving columns and
/// primary-key membership by the raw column names the transactions carry.
/// </summary>
public static class SchemaTableExtensions
{
    /// <summary>
    /// Finds a column on the table by its raw name.
    /// </summary>
    /// <param name="table">The observed table to look in.</param>
    /// <param name="name">The raw column name.</param>
    /// <returns>The <see cref="SchemaColumn"/>, or <see langword="null"/> when the table has no such column.</returns>
    public static SchemaColumn? Column(this SchemaTable table, string name) =>
        table.Columns.FirstOrDefault(column => string.Equals(column.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Decides whether a column is part of the table's primary key.
    /// </summary>
    /// <param name="table">The observed table to check.</param>
    /// <param name="name">The raw column name.</param>
    /// <returns>Whether the column is part of the primary key.</returns>
    public static bool IsPrimaryKey(this SchemaTable table, string name) =>
        table.PrimaryKey.Contains(name, StringComparer.OrdinalIgnoreCase);
}
