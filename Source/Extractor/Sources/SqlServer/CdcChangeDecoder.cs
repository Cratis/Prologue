// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.SqlServer;

/// <summary>
/// Decodes raw SQL Server CDC rows into per-transaction observations, deriving the changed column names from the
/// CDC update mask. This is pure metadata extraction — no data values are read.
/// </summary>
public static class CdcChangeDecoder
{
    const int Delete = 1;
    const int Insert = 2;
    const int UpdateBefore = 3;
    const int UpdateAfter = 4;

    /// <summary>
    /// Groups raw CDC rows by their commit LSN into one observation per transaction.
    /// </summary>
    /// <param name="rows">The raw CDC rows to group.</param>
    /// <param name="source">The source kind the rows came from.</param>
    /// <param name="database">The name of the database the transactions occurred in.</param>
    /// <returns>One <see cref="Observation"/> per transaction, anchored to the transaction's commit time.</returns>
    public static IReadOnlyList<Observation> GroupIntoTransactions(
        IEnumerable<CdcRow> rows,
        SourceKind source,
        string database)
    {
        var observations = new List<Observation>();

        var transactions = rows
            .Where(row => row.Operation != UpdateBefore)
            .GroupBy(row => Convert.ToHexString(row.StartLsn));

        foreach (var transaction in transactions)
        {
            var tables = transaction
                .Select(row => new TableChange(
                    row.Schema,
                    row.Table,
                    MapOperation(row.Operation),
                    ChangedColumns(row)))
                .ToList();

            var payload = new DatabaseTransactionObserved(source.Value, database, transaction.Key, tables);
            observations.Add(new Observation(source, transaction.First().CommitTime, payload));
        }

        return observations;
    }

    /// <summary>
    /// Determines the names of the columns a CDC row changed. Inserts and deletes change the whole row; updates
    /// change only the columns flagged in the update mask.
    /// </summary>
    /// <param name="row">The CDC row to inspect.</param>
    /// <returns>The names of the changed columns.</returns>
    public static IReadOnlyList<string> ChangedColumns(CdcRow row) =>
        row.Operation == UpdateAfter
            ? [.. row.Columns.Where((_, index) => IsBitSet(row.UpdateMask, index + 1))]
            : row.Columns;

    /// <summary>
    /// Determines whether the bit for a given 1-based column ordinal is set in a CDC update mask. SQL Server stores
    /// the mask so that ordinal 1 is the least significant bit of the last byte.
    /// </summary>
    /// <param name="mask">The CDC update mask.</param>
    /// <param name="ordinal">The 1-based column ordinal.</param>
    /// <returns>True if the column's bit is set; otherwise false.</returns>
    public static bool IsBitSet(byte[] mask, int ordinal)
    {
        var bitIndex = ordinal - 1;
        var byteFromEnd = bitIndex / 8;
        if (byteFromEnd >= mask.Length)
        {
            return false;
        }

        var value = mask[mask.Length - 1 - byteFromEnd];
        return (value & (1 << (bitIndex % 8))) != 0;
    }

    static ChangeOperation MapOperation(int operation) => operation switch
    {
        Insert => ChangeOperation.Insert,
        Delete => ChangeOperation.Delete,
        _ => ChangeOperation.Update
    };
}
