// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

/// <summary>
/// Represents a SQL Server CDC capture instance — the change table tracking one source table — and the ordered
/// names of the columns it captures.
/// </summary>
/// <param name="Name">The capture instance name (the <c>cdc.&lt;name&gt;_CT</c> change table).</param>
/// <param name="Schema">The schema of the source table.</param>
/// <param name="Table">The name of the source table.</param>
/// <param name="Columns">The captured column names, ordered by column ordinal.</param>
public record CdcCaptureInstance(string Name, string Schema, string Table, IReadOnlyList<string> Columns);
