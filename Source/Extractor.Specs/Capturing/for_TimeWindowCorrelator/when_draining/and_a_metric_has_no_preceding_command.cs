// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

public class and_a_metric_has_no_preceding_command : a_correlator
{
    IReadOnlyList<Capture> _result;

    // Metrics carry no trace id, so there is nothing to group them by — each settles into its own capture.
    void Establish()
    {
        _correlator.Add(Metric(_origin));
        _correlator.Add(Metric(_origin + TimeSpan.FromMilliseconds(100), "db.client.operation.duration"));
    }

    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromSeconds(5));

    [Fact] void should_produce_a_capture_per_metric() => _result.Count.ShouldEqual(2);
    [Fact] void should_carry_a_single_observation_each() => _result.ShouldContainOnly(_result.Where(capture => capture.Entries.Count == 1));
    [Fact] void should_stamp_the_captures_with_the_configured_prologue() => _result.ShouldContainOnly(_result.Where(capture => capture.PrologueId == _prologueId));
}
#endif
