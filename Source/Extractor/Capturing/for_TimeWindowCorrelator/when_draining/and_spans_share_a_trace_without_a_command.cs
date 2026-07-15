// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

public class and_spans_share_a_trace_without_a_command : a_correlator
{
    IReadOnlyList<Capture> _result;

    void Establish()
    {
        _correlator.Add(Span(_origin, "feedfeedfeedfeedfeedfeedfeedfe02", "PlaceOrder"));
        _correlator.Add(Span(_origin + TimeSpan.FromMilliseconds(100), "feedfeedfeedfeedfeedfeedfeedfe02", "SaveOrder"));
    }

    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromSeconds(5));

    [Fact] void should_group_the_trace_into_a_single_capture() => _result.Count.ShouldEqual(1);
    [Fact] void should_contain_both_spans() => _result[0].Entries.Count.ShouldEqual(2);
}
#endif
