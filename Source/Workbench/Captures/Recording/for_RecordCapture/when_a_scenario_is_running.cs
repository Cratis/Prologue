// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios;
using Cratis.Prologue.Workbench.Scenarios.Marking;

namespace Cratis.Prologue.Workbench.Captures.Recording.for_RecordCapture;

public class when_a_scenario_is_running : Specification
{
    CaptureRecorded _recorded;

    void Because()
    {
        var prologue = PrologueId.New();
        var script = ScenarioScript.Reduce(new ScenarioStarted("Lend a copy"), null, EventContext.EmptyWithEventSourceId(prologue));
        _recorded = Command(prologue).Handle(script);
    }

    [Fact] void should_label_the_capture_with_the_scenario() => _recorded.Scenario.Value.ShouldEqual("Lend a copy");
    [Fact] void should_keep_what_was_captured() => _recorded.Http.Shape.Value.ShouldEqual("POST /api/loans");

    internal static RecordCapture Command(PrologueId prologue) =>
        new(prologue, Guid.NewGuid(), DateTimeOffset.UtcNow, ServiceName.Default, new RecordedCommand("POST", "/api/loans", 201, TraceId.None, "POST /api/loans", 0), [], 0, [], false);
}
#endif
