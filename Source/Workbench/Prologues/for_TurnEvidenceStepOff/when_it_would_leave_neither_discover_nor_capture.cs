// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.Evidence;
using Cratis.Prologue.Workbench.Prologues.given;

namespace Cratis.Prologue.Workbench.Prologues.for_TurnEvidenceStepOff;

public class when_it_would_leave_neither_discover_nor_capture : a_prologue
{
    Result<EvidenceStepTurnedOff, ValidationResult> _result;

    void Because()
    {
        var discoverOff = PrologueDefinition.Reduce(new EvidenceStepTurnedOff(EvidenceStep.Discover), _prologue);
        _result = new TurnEvidenceStepOff(_id, EvidenceStep.Capture).Handle(discoverOff);
    }

    [Fact] void should_refuse() => _result.Appended().ShouldBeNull();
    [Fact] void should_explain_that_some_evidence_is_needed() => _result.Refusal().Message.ShouldContain("Keep Discover or Capture on");
}
#endif
