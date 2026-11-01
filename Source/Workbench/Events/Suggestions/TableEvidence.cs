// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A change to a database table that evidences a suggestion.
/// </summary>
/// <param name="Schema">The schema of the table.</param>
/// <param name="Table">The table.</param>
/// <param name="Operation">The operation, one of Insert, Update or Delete.</param>
/// <param name="Columns">The columns the change wrote.</param>
public record TableEvidence(string Schema, string Table, string Operation, IEnumerable<string> Columns);
