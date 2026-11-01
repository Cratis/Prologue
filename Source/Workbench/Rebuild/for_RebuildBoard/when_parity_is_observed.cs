// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Rebuild.given;

namespace Cratis.Prologue.Workbench.Rebuild.for_RebuildBoard;

public class when_parity_is_observed : a_board
{
    void Because()
    {
        Apply(Observation("POST /api/loans", ParityOutcome.Matched));
        Apply(Observation("POST /api/loans", ParityOutcome.Matched));
        Apply(Observation("POST /api/loans", ParityOutcome.Mismatched, legacy: 201, shadow: 409));
        Apply(Observation("POST /api/loans", ParityOutcome.Unavailable, shadow: 0));
        Apply(Observation("POST /api/loans", ParityOutcome.Skipped, shadow: 0));
        Apply(Observation("POST /api/members", ParityOutcome.Matched));
        Apply(Observation("POST /api/loans/{id}/return", ParityOutcome.Matched, path: "/api/loans/9/return"));
    }

    [Fact] void should_count_matching_decisions() => Lend.Matched.ShouldEqual(2);
    [Fact] void should_count_differing_decisions() => Lend.Mismatched.ShouldEqual(1);
    [Fact] void should_count_an_unavailable_shadow_apart_from_mismatches() => Lend.Unavailable.ShouldEqual(1);
    [Fact] void should_count_skipped_commands_apart() => Lend.Skipped.ShouldEqual(1);
    [Fact] void should_compare_only_decisions_that_were_made() => Lend.Compared().ShouldEqual(3);
    [Fact] void should_hold_each_difference_open_until_resolved() => Lend.OpenDifferences.Count().ShouldEqual(1);
    [Fact] void should_count_for_the_slice_that_replaces_the_shape() => Return.Matched.ShouldEqual(1);
    [Fact] void should_count_nothing_for_a_shape_no_slice_replaces() => (Lend.Matched + Return.Matched).ShouldEqual(3);
}
#endif
