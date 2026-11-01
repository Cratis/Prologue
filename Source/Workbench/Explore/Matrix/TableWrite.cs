// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using TableName = Cratis.Prologue.Workbench.Systems.TableName;

namespace Cratis.Prologue.Workbench.Explore.Matrix;

/// <summary>
/// What captures did to one table: the kinds of change with how many runs they happened in, and the columns that changed.
/// </summary>
/// <param name="Schema">The schema of the table.</param>
/// <param name="Table">The table.</param>
/// <param name="Operations">The kinds of change seen, in the order insert, update, delete.</param>
/// <param name="Columns">Every column that changed, in the order first seen.</param>
public record TableWrite(SchemaName Schema, TableName Table, IEnumerable<OperationRuns> Operations, IEnumerable<ColumnName> Columns);
