// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Interpreting.given;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationRunner;

public class when_there_is_nothing_to_interpret : an_interpretation_runner
{
    InterpretationProgress _noCaptures;
    InterpretationProgress _nothingWaits;
    int _recordedWhenThereWasNothing;

    async Task Because()
    {
        _captures.Clear();
        await Run();
        _noCaptures = Progress;
        _recordedWhenThereWasNothing = _recorded.Count;
        await Run(resume: true);
        _nothingWaits = Progress;
    }

    [Fact] void should_fail_with_a_reason_a_person_can_act_on() => _noCaptures.Message.ShouldContain("no captures");
    [Fact] void should_fail() => _noCaptures.State.ShouldEqual(InterpretationState.Failed);
    [Fact] void should_record_nothing() => _recordedWhenThereWasNothing.ShouldEqual(0);
    [Fact] void should_fail_resuming_when_nothing_waits() => _nothingWaits.State.ShouldEqual(InterpretationState.Failed);
}
#endif
