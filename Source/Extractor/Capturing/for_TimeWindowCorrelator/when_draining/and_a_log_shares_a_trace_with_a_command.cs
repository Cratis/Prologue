// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

public class and_a_log_shares_a_trace_with_a_command : a_correlator
{
    IReadOnlyList<Capture> _result;

    void Establish()
    {
        _correlator.Add(Command(_origin, "abcabcabcabcabcabcabcabcabcabc01"));

        // The log record occurs after the correlation window but shares the command's trace id.
        _correlator.Add(Log(_origin + TimeSpan.FromMilliseconds(1500), "abcabcabcabcabcabcabcabcabcabc01"));
    }

    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromSeconds(5));

    [Fact] void should_produce_a_single_capture() => _result.Count.ShouldEqual(1);
    [Fact] void should_correlate_the_log_with_the_command_by_trace_id() => _result[0].Entries.Count.ShouldEqual(2);
    [Fact] void should_stamp_the_capture_with_the_configured_prologue() => _result[0].PrologueId.ShouldEqual(_prologueId);
}
#endif
