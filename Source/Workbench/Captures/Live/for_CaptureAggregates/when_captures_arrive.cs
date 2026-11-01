// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Captures.Live.for_CaptureAggregates;

public class when_captures_arrive : Specification
{
    CaptureAggregates _aggregates;

    void Because()
    {
        var context = EventContext.EmptyWithEventSourceId(PrologueId.New());
        var later = RecordedCaptures.Start.AddMinutes(1);
        CaptureRecorded[] events =
        [
            RecordedCaptures.Command("POST", "/api/loans", 201, [RecordedCaptures.Write(ChangeOperation.Insert, "Loans", "BookId"), RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "AvailableCount")], "Lend a copy", traceLinked: true),
            RecordedCaptures.Command("POST", "/api/loans", 409, scenario: "Lend a copy"),
            RecordedCaptures.Command("POST", "/api/loans/7/return", 200, [RecordedCaptures.Write(ChangeOperation.Update, "Loans", "ReturnedOn")], occurred: later),
            RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "TotalCount"), RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "TotalCount"))
        ];

        foreach (var @event in events)
        {
            _aggregates = CaptureAggregates.Reduce(@event, _aggregates, context);
        }
    }

    [Fact] void should_count_every_capture() => _aggregates.Total.ShouldEqual(4);
    [Fact] void should_count_the_captures_with_a_command() => _aggregates.WithCommand.ShouldEqual(3);
    [Fact] void should_count_the_trace_linked_ones() => _aggregates.TraceLinked.ShouldEqual(1);
    [Fact] void should_count_transactions_with_no_command() => _aggregates.UncorrelatedTransactions.ShouldEqual(2);
    [Fact] void should_count_the_captures_touching_each_table() => _aggregates.Tables.Single(table => table.Table.Value == "Loans").Captures.ShouldEqual(2);
    [Fact] void should_count_a_table_touched_twice_in_one_capture_once() => _aggregates.Tables.Single(table => table.Table.Value == "Inventory").Captures.ShouldEqual(2);
    [Fact] void should_bucket_captures_per_minute() => _aggregates.PerMinute.Select(bucket => bucket.Captures).ShouldContainOnly([3, 1]);
    [Fact] void should_keep_the_minutes_oldest_first() => _aggregates.PerMinute.First().Minute.ShouldBeLessThan(_aggregates.PerMinute.Last().Minute);
    [Fact] void should_count_the_captures_per_scenario() => _aggregates.Scenarios.Single().Captures.ShouldEqual(2);
}
#endif
