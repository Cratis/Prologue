// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Scenarios.given;

namespace Cratis.Prologue.Workbench.Scenarios.Marking.for_MarkScenarioStarted;

public class when_another_scenario_is_running : a_script
{
    Result<ScenarioStarted, ValidationResult> _overlapping;
    Result<ScenarioStarted, ValidationResult> _sameAgain;

    void Establish() => _script = ScenarioScript.Reduce(new ScenarioStarted("Lend a copy"), _script, _context);

    void Because()
    {
        _overlapping = new MarkScenarioStarted(_prologue, "Return a copy").Handle(_script);
        _sameAgain = new MarkScenarioStarted(_prologue, "Lend a copy").Handle(_script);
    }

    [Fact] void should_refuse_an_overlapping_scenario() => _overlapping.Refusal().ShouldNotBeNull();
    [Fact] void should_say_which_scenario_is_running() => _overlapping.Refusal()!.Message.ShouldContain("Lend a copy");
    [Fact] void should_refuse_starting_the_same_one_again() => _sameAgain.Refusal().ShouldNotBeNull();
}
#endif
