// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.SqlServer.for_CdcChangeDecoder.when_decoding_changed_columns;

public class and_operation_is_an_update : Specification
{
    CdcRow _row;
    IReadOnlyList<string> _result;

    // Mask 0x05 = bits for ordinal 1 and ordinal 3 set; ordinal 2 unset.
    void Establish() => _row = new CdcRow([0, 0, 0, 0, 0, 0, 0, 0, 0, 1], DateTimeOffset.UtcNow, "dbo", "Customers", 4, [0x05], ["Id", "Name", "Email"]);

    void Because() => _result = CdcChangeDecoder.ChangedColumns(_row);

    [Fact] void should_only_include_the_flagged_columns() => _result.ShouldContainOnly(["Id", "Email"]);
}
#endif
