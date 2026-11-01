// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Captures.Live;

/// <summary>
/// A table captures changed.
/// </summary>
/// <param name="Schema">The schema of the table.</param>
/// <param name="Table">The table.</param>
/// <param name="Captures">How many captures changed it.</param>
public record TableTouched(SchemaName Schema, TableName Table, int Captures);
