// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

public class and_a_command_is_followed_by_a_transaction : a_correlator
{
    IReadOnlyList<Capture> _result;

    void Establish()
    {
        _correlator.Add(Command(_origin));
        _correlator.Add(Transaction(_origin + TimeSpan.FromMilliseconds(200)));
    }

    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromSeconds(5));

    [Fact] void should_produce_a_single_capture() => _result.Count.ShouldEqual(1);
    [Fact] void should_correlate_both_observations() => _result[0].Entries.Count.ShouldEqual(2);
    [Fact] void should_anchor_the_capture_to_the_command_time() => _result[0].Occurred.ShouldEqual(_origin);
}
#endif
