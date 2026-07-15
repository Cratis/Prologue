// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.given;

namespace Cratis.Prologue.Extractor.Capturing.for_TimeWindowCorrelator.when_draining;

public class and_observations_have_not_settled : a_correlator
{
    IReadOnlyList<Capture> _result;

    void Establish() => _correlator.Add(Command(_origin));

    // Draining only half a window after the command means more correlated observations could still arrive.
    void Because() => _result = _correlator.Drain(_origin + TimeSpan.FromMilliseconds(500));

    [Fact] void should_produce_no_captures() => _result.ShouldBeEmpty();
}
#endif
