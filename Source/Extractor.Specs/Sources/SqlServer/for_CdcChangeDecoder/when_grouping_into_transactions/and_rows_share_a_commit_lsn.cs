// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.SqlServer.for_CdcChangeDecoder.when_grouping_into_transactions;

public class and_rows_share_a_commit_lsn : Specification
{
    readonly byte[] _lsn = [0, 0, 0, 0, 0, 0, 0, 0, 0, 7];
    IReadOnlyList<Observation> _result;

    void Because() => _result = CdcChangeDecoder.GroupIntoTransactions(
    [
        new CdcRow(_lsn, DateTimeOffset.UtcNow, "dbo", "Orders", 2, [0x00], ["Id"]),
        new CdcRow(_lsn, DateTimeOffset.UtcNow, "dbo", "OrderLines", 2, [0x00], ["Id"])
    ],
    SourceKind.SqlServer,
    "Shop");

    [Fact] void should_produce_a_single_transaction() => _result.Count.ShouldEqual(1);
    [Fact]
    void should_include_both_changed_tables() =>
        ((DatabaseTransactionObserved)_result[0].Payload).Tables.Select(_ => _.Table).ShouldContainOnly(["Orders", "OrderLines"]);
}
#endif
