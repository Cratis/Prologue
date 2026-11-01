// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// A change to one table inside a captured database transaction. Only the columns that changed are known, never the values.
/// </summary>
/// <param name="Schema">The schema of the table.</param>
/// <param name="Table">The table.</param>
/// <param name="Operation">What was done to a row.</param>
/// <param name="Columns">The columns that changed.</param>
public record RecordedTableChange(SchemaName Schema, Systems.TableName Table, ChangeOperation Operation, IEnumerable<ColumnName> Columns);
