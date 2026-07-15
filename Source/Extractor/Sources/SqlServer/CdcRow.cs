// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

/// <summary>
/// Represents a single raw row read from a SQL Server CDC change table, carrying only the metadata Prologue needs.
/// </summary>
/// <param name="StartLsn">The commit LSN of the transaction the row belongs to. Rows sharing a value belong to the same transaction.</param>
/// <param name="CommitTime">The time the transaction committed.</param>
/// <param name="Schema">The schema of the source table.</param>
/// <param name="Table">The name of the source table.</param>
/// <param name="Operation">The CDC operation code (1 = delete, 2 = insert, 3 = update before-image, 4 = update after-image).</param>
/// <param name="UpdateMask">The CDC update bitmask identifying which columns changed (for updates).</param>
/// <param name="Columns">The ordered captured column names for the source table (ordinal 1 first).</param>
public record CdcRow(
    byte[] StartLsn,
    DateTimeOffset CommitTime,
    string Schema,
    string Table,
    int Operation,
    byte[] UpdateMask,
    IReadOnlyList<string> Columns);
