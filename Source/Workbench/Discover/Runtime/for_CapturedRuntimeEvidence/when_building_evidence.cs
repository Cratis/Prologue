// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Live;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Explore.given;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Runtime.for_CapturedRuntimeEvidence;

public class when_building_evidence : Specification
{
    RuntimeEvidence _evidence;
    RuntimeEvidence _nothingCaptured;

    void Because()
    {
        var events = new[]
        {
            Captured.Command("POST", "/api/loans", 201, "LoansController.Lend"),
            Captured.Command("POST", "/api/loans", 201, "LoansController.Lend"),
            Captured.Command("POST", "/api/loans", 409, "LoansController.Lend"),
            Captured.Command("GET", "/api/books", 200, "BooksController.List"),
            RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "TotalCount"))
        };
        var context = EventContext.EmptyWithEventSourceId(PrologueId.New());
        CaptureAggregates? aggregates = null;
        foreach (var @event in events)
        {
            aggregates = CaptureAggregates.Reduce(@event, aggregates, context);
        }

        _evidence = CapturedRuntimeEvidence.From(Captured.Matrix(events), aggregates);
        _nothingCaptured = CapturedRuntimeEvidence.From(null, null);
    }

    RuntimeShapeActivity Loans => _evidence.Find("POST /api/loans")!;

    [Fact] void should_have_the_activity_of_each_captured_command_shape() => _evidence.Shapes.Select(shape => shape.Shape.Value).ShouldContainOnly("POST /api/loans", "GET /api/books");
    [Fact] void should_count_the_captures_per_status() => Loans.Statuses.Select(status => (status.Status, status.Count)).ShouldContainOnly([(201, 2), (409, 1)]);
    [Fact] void should_count_a_refusal() => Loans.Refused.ShouldEqual(1);
    [Fact] void should_count_the_uncorrelated_transactions() => _evidence.UncorrelatedTransactions.ShouldEqual(1);
    [Fact] void should_be_empty_when_nothing_was_captured() => (_nothingCaptured.Shapes.Count(), _nothingCaptured.UncorrelatedTransactions).ShouldEqual((0, 0));
}
#endif
