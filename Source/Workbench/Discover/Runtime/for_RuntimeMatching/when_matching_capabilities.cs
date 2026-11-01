// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover.given;

namespace Cratis.Prologue.Workbench.Discover.Runtime.for_RuntimeMatching;

public class when_matching_capabilities : Specification
{
    static readonly RuntimeEvidence _evidence = new(
        [
            new(new RouteShape("POST /api/loans/{id}/return"), [new(200, 880)]),
            new(new RouteShape("POST /api/loans"), [new(200, 902), new(409, 72)])
        ],
        0);

    int _seen;
    int _seenInAnotherCase;
    int _never;
    int _noCommand;
    int _combined;

    void Because()
    {
        _seen = RuntimeMatching.Seen(Sample.Capability("Loans", "Return loan", "POST /api/loans/{id}/return").Shapes, _evidence);
        _seenInAnotherCase = RuntimeMatching.Seen(Sample.Capability("Loans", "Return loan", "post /API/loans/{id}/return").Shapes, _evidence);
        _never = RuntimeMatching.Seen(Sample.Capability("Loans", "Renew", "POST /api/loans/{id}/renew").Shapes, _evidence);
        _noCommand = RuntimeMatching.Seen(Sample.Capability("Jobs", "Sweep").Shapes, _evidence);
        _combined = RuntimeMatching.Seen(Sample.Capability("Loans", "Lend or return", "POST /api/loans", "POST /api/loans/{id}/return").Shapes, _evidence);
    }

    [Fact] void should_count_the_captures_of_the_command_shape() => _seen.ShouldEqual(880);
    [Fact] void should_ignore_case_in_the_path() => _seenInAnotherCase.ShouldEqual(880);
    [Fact] void should_not_see_a_command_that_was_never_captured() => _never.ShouldEqual(0);
    [Fact] void should_not_see_a_capability_no_command_reaches() => _noCommand.ShouldEqual(0);
    [Fact] void should_add_up_every_shape_of_a_capability() => _combined.ShouldEqual(880 + 974);
}
#endif
