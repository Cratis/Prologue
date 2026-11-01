// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Scenarios.given;

namespace Cratis.Prologue.Workbench.Scenarios.Marking.for_MarkScenarioEnded;

public class when_ending : a_script
{
    Result<ScenarioEnded, ValidationResult> _nothingRunning;
    Result<ScenarioEnded, ValidationResult> _running;
    ScenarioScript _afterEnd;

    void Because()
    {
        var started = ScenarioScript.Reduce(new ScenarioStarted("Lend a copy"), _script, _context);
        _nothingRunning = new MarkScenarioEnded(_prologue).Handle(_script);
        _running = new MarkScenarioEnded(_prologue).Handle(started);
        _afterEnd = ScenarioScript.Reduce(_running.Appended()!, started, _context);
    }

    [Fact] void should_refuse_when_nothing_is_running() => _nothingRunning.Refusal().ShouldNotBeNull();
    [Fact] void should_end_the_running_scenario() => _running.Appended()!.Name.Value.ShouldEqual("Lend a copy");
    [Fact] void should_leave_no_scenario_running() => _afterEnd.Active.ShouldEqual(ScenarioName.None);
    [Fact] void should_allow_starting_another_afterwards() => new MarkScenarioStarted(_prologue, "Return a copy").Handle(_afterEnd).Appended().ShouldNotBeNull();
}
#endif
