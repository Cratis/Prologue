// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Captures.Live.for_ShapeCoverage;

public class when_captures_arrive : Specification
{
    ShapeCoverage _coverage;

    void Because()
    {
        var context = EventContext.EmptyWithEventSourceId(PrologueId.New());
        CaptureRecorded[] events =
        [
            RecordedCaptures.Command("POST", "/api/loans/1/return", 200),
            RecordedCaptures.Command("POST", "/api/loans/2/return", 200, scenario: "Return a copy"),
            RecordedCaptures.Command("POST", "/api/loans", 409),
            RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "TotalCount"))
        ];

        foreach (var @event in events)
        {
            _coverage = ShapeCoverage.Reduce(@event, _coverage, context);
        }
    }

    ShapeCount Shape(string shape) => _coverage.Shapes.Single(entry => entry.Shape.Value == shape);

    [Fact] void should_group_captures_by_shape() => Shape("POST /api/loans/{id}/return").Captures.ShouldEqual(2);
    [Fact] void should_not_group_captures_without_a_command() => _coverage.Shapes.Count().ShouldEqual(2);
    [Fact] void should_count_accepted_and_refused() => (Shape("POST /api/loans").Accepted, Shape("POST /api/loans").Refused).ShouldEqual((0, 1));
    [Fact] void should_remember_the_latest_scenario() => Shape("POST /api/loans/{id}/return").LastScenario.Value.ShouldEqual("Return a copy");
}
#endif
