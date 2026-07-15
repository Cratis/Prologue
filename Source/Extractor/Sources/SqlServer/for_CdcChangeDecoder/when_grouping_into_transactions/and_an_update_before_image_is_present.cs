// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.SqlServer.for_CdcChangeDecoder.when_grouping_into_transactions;

public class and_an_update_before_image_is_present : Specification
{
    readonly byte[] _lsn = [0, 0, 0, 0, 0, 0, 0, 0, 0, 9];
    IReadOnlyList<Observation> _result;

    // Operation 3 is the update before-image and must be ignored so updates are not double-counted.
    void Because() => _result = CdcChangeDecoder.GroupIntoTransactions(
    [
        new CdcRow(_lsn, DateTimeOffset.UtcNow, "dbo", "Customers", 3, [0x01], ["Name"]),
        new CdcRow(_lsn, DateTimeOffset.UtcNow, "dbo", "Customers", 4, [0x01], ["Name"])
    ],
    SourceKind.SqlServer,
    "Shop");

    [Fact]
    void should_record_the_table_change_once() =>
        ((DatabaseTransactionObserved)_result[0].Payload).Tables.Count.ShouldEqual(1);
    [Fact]
    void should_record_it_as_an_update() =>
        ((DatabaseTransactionObserved)_result[0].Payload).Tables[0].Operation.ShouldEqual(ChangeOperation.Update);
}
#endif
