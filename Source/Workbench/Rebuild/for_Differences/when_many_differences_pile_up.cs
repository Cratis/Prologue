// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Rebuild.Differences;
using Cratis.Prologue.Workbench.Rebuild.given;

namespace Cratis.Prologue.Workbench.Rebuild.for_Differences;

public class when_many_differences_pile_up : a_board
{
    ObservationId _oldestOpen;

    void Because()
    {
        var oldest = Observation("POST /api/loans", ParityOutcome.Mismatched, shadow: 409);
        _oldestOpen = oldest.Observation;
        Apply(oldest);
        for (var i = 0; i < RebuildDifferences.ResolvedKept + 20; i++)
        {
            var observed = Observation("POST /api/loans", ParityOutcome.Mismatched, shadow: 409);
            Apply(observed);
            _differences = RebuildDifferences.Reduce(new DifferenceAccepted(observed.Observation, observed.Shape), _differences, _context);
        }
    }

    [Fact] void should_keep_only_so_many_resolved_differences() => _differences!.Differences.Count(difference => difference.Resolution == DifferenceResolution.Accepted).ShouldEqual(RebuildDifferences.ResolvedKept);
    [Fact] void should_never_drop_an_open_difference() => _differences!.Differences.Select(difference => difference.Observation).ShouldContain(_oldestOpen);
}
#endif
