// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents a foreign-key relationship from an observed table to the table it references — the evidence of how
/// entities in the captured system relate to each other.
/// </summary>
/// <param name="Name">The name of the foreign-key constraint.</param>
/// <param name="Columns">The names of the referencing columns, in constraint order.</param>
/// <param name="ReferencedSchema">The schema of the referenced table.</param>
/// <param name="ReferencedTable">The name of the referenced table.</param>
/// <param name="ReferencedColumns">The names of the referenced columns, in the same order as <paramref name="Columns"/>.</param>
public record SchemaForeignKey(
    string Name,
    IReadOnlyList<string> Columns,
    string ReferencedSchema,
    string ReferencedTable,
    IReadOnlyList<string> ReferencedColumns);
