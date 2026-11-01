// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.Evidence;
using Cratis.Prologue.Workbench.Prologues.given;

namespace Cratis.Prologue.Workbench.Prologues.for_TurnEvidenceStepOff;

public class when_capture_is_on : a_prologue
{
    EvidenceStepTurnedOff _event;

    void Because() => _event = new TurnEvidenceStepOff(_id, EvidenceStep.Discover).Handle(_prologue).Appended();

    [Fact] void should_turn_discover_off() => _event.Step.ShouldEqual(EvidenceStep.Discover);
}
#endif
