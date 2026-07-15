// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.SqlServer.for_CdcChangeDecoder.when_decoding_changed_columns;

public class and_operation_is_an_insert : Specification
{
    CdcRow _row;
    IReadOnlyList<string> _result;

    // Mask is empty for inserts; the whole row is new so every column counts as changed.
    void Establish() => _row = new CdcRow([0, 0, 0, 0, 0, 0, 0, 0, 0, 1], DateTimeOffset.UtcNow, "dbo", "Customers", 2, [0x00], ["Id", "Name", "Email"]);

    void Because() => _result = CdcChangeDecoder.ChangedColumns(_row);

    [Fact] void should_include_every_column() => _result.ShouldContainOnly(["Id", "Name", "Email"]);
}
#endif
