// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the metadata for a change to a single table within a database transaction — the table and the
/// names of the columns that changed. No data values are captured.
/// </summary>
/// <param name="Schema">The schema the table belongs to.</param>
/// <param name="Table">The name of the table that changed.</param>
/// <param name="Operation">The kind of change that was applied.</param>
/// <param name="Columns">The names of the columns that changed.</param>
public record TableChange(string Schema, string Table, ChangeOperation Operation, IReadOnlyList<string> Columns);
