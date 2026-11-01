// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios;
using Cratis.Prologue.Workbench.Scenarios.Marking;

namespace Cratis.Prologue.Workbench.Captures.Recording.for_RecordCapture;

public class when_no_scenario_is_running : Specification
{
    CaptureRecorded _withoutScript;
    CaptureRecorded _afterTheScenarioEnded;

    void Because()
    {
        var prologue = PrologueId.New();
        var context = EventContext.EmptyWithEventSourceId(prologue);
        var started = ScenarioScript.Reduce(new ScenarioStarted("Lend a copy"), null, context);
        var ended = ScenarioScript.Reduce(new ScenarioEnded("Lend a copy"), started, context);

        _withoutScript = when_a_scenario_is_running.Command(prologue).Handle(null);
        _afterTheScenarioEnded = when_a_scenario_is_running.Command(prologue).Handle(ended);
    }

    [Fact] void should_not_label_a_capture_before_any_scenario() => _withoutScript.Scenario.ShouldEqual(ScenarioName.None);
    [Fact] void should_not_label_a_capture_after_the_scenario_ended() => _afterTheScenarioEnded.Scenario.ShouldEqual(ScenarioName.None);
}
#endif
