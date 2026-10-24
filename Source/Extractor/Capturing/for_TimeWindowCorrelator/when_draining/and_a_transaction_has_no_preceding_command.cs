// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

public class and_a_transaction_has_no_preceding_command : a_correlator
{
    IReadOnlyList<Capture> _result;

    void Establish() => _correlator.Add(Transaction(_origin));

    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromSeconds(5));

    [Fact] void should_produce_a_standalone_capture() => _result.Count.ShouldEqual(1);
    [Fact] void should_contain_only_the_transaction() => _result[0].Entries.Count.ShouldEqual(1);
}
#endif
