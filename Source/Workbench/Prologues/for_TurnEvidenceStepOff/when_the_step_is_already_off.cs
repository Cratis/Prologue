// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.Evidence;
using Cratis.Prologue.Workbench.Prologues.given;

namespace Cratis.Prologue.Workbench.Prologues.for_TurnEvidenceStepOff;

public class when_the_step_is_already_off : a_prologue
{
    Result<EvidenceStepTurnedOff, ValidationResult> _result;

    void Because()
    {
        var rebuildOff = PrologueDefinition.Reduce(new EvidenceStepTurnedOff(EvidenceStep.Rebuild), _prologue);
        _result = new TurnEvidenceStepOff(_id, EvidenceStep.Rebuild).Handle(rebuildOff);
    }

    [Fact] void should_not_record_it_again() => _result.Appended().ShouldBeNull();
}
#endif
