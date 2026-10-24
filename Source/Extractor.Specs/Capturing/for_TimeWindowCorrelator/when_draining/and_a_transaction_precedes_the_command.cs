// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

public class and_a_transaction_precedes_the_command : a_correlator
{
    IReadOnlyList<Capture> _result;

    void Establish()
    {
        _correlator.Add(Transaction(_origin));
        _correlator.Add(Command(_origin + TimeSpan.FromMilliseconds(500)));
    }

    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromSeconds(5));

    // A transaction committed before the command cannot have been caused by it; they stay separate.
    [Fact] void should_not_correlate_them() => _result.Count.ShouldEqual(2);
    [Fact] void should_keep_each_capture_to_a_single_entry() => _result.All(_ => _.Entries.Count == 1).ShouldBeTrue();
}
#endif
