// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Scenarios.given;
using Cratis.Prologue.Workbench.Scenarios.Marking;

namespace Cratis.Prologue.Workbench.Scenarios.Scripting.for_RemoveScenario;

public class when_removing : a_script
{
    Result<ScenarioRemoved, ValidationResult> _known;
    Result<ScenarioRemoved, ValidationResult> _unknown;
    Result<ScenarioRemoved, ValidationResult> _running;
    ScenarioScript _afterRemoval;

    void Because()
    {
        var started = ScenarioScript.Reduce(new ScenarioStarted("Lend a copy"), _script, _context);
        _known = new RemoveScenario(_prologue, "Return a copy").Handle(_script);
        _unknown = new RemoveScenario(_prologue, "Nope").Handle(_script);
        _running = new RemoveScenario(_prologue, "Lend a copy").Handle(started);
        _afterRemoval = ScenarioScript.Reduce(_known.Appended()!, _script, _context);
    }

    [Fact] void should_remove_a_scenario_in_the_script() => _known.Appended().ShouldNotBeNull();
    [Fact] void should_refuse_a_scenario_not_in_the_script() => _unknown.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_removing_the_running_scenario() => _running.Refusal().ShouldNotBeNull();
    [Fact] void should_leave_the_other_scenarios() => _afterRemoval.Scenarios.Select(name => name.Value).ShouldContainOnly(["Lend a copy"]);
}
#endif
