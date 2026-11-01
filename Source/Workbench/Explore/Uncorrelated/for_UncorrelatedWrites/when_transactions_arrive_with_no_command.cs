// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Explore.given;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Uncorrelated.for_UncorrelatedWrites;

public class when_transactions_arrive_with_no_command : Specification
{
    UncorrelatedWrites _writes;

    void Because()
    {
        var context = EventContext.EmptyWithEventSourceId(PrologueId.New());
        var night = new DateTimeOffset(2026, 11, 1, 2, 3, 0, TimeSpan.Zero);
        CaptureRecorded[] events =
        [
            RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "TotalCount")) with { Occurred = night },
            RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "AvailableCount")) with { Occurred = night.AddDays(1) },
            RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Delete, "Inventory", "Id")) with { Occurred = night.AddHours(1) },
            RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Insert, "AuditLog", "Entry")) with { Occurred = night.AddHours(9) },
            Captured.Command("POST", "/api/loans", 201, "", "", RecordedCaptures.Write(ChangeOperation.Insert, "Loans", "BookId")),
            RecordedCaptures.WithoutCommand()
        ];

        foreach (var @event in events)
        {
            _writes = UncorrelatedWrites.Reduce(@event, _writes, context);
        }
    }

    UncorrelatedTable Table(string name) => _writes.Tables.Single(table => table.Write.Table.Value == name);

    [Fact] void should_group_by_table() => _writes.Tables.Select(table => table.Write.Table.Value).ShouldContainOnly(["Inventory", "AuditLog"]);
    [Fact] void should_ignore_captures_that_hold_a_command() => _writes.Tables.Any(table => table.Write.Table.Value == "Loans").ShouldBeFalse();
    [Fact] void should_count_the_transactions_per_table() => Table("Inventory").Transactions.ShouldEqual(3);
    [Fact] void should_count_the_hours_of_the_day() => Table("Inventory").Hours.Select(hour => (hour.Hour, hour.Transactions)).ShouldContainOnly([(2, 2), (3, 1)]);
    [Fact] void should_count_the_operations_per_transaction() => Table("Inventory").Write.Operations.Select(runs => (runs.Operation, runs.Runs)).ShouldContainOnly([(ChangeOperation.Update, 2), (ChangeOperation.Delete, 1)]);
    [Fact] void should_keep_the_columns() => Table("Inventory").Write.Columns.Select(column => column.Value).ShouldContainOnly(["TotalCount", "AvailableCount", "Id"]);
    [Fact] void should_ignore_a_capture_with_nothing_in_it() => Table("AuditLog").Transactions.ShouldEqual(1);
}
#endif
