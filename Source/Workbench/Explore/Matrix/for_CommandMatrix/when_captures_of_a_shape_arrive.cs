// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Explore.given;

namespace Cratis.Prologue.Workbench.Explore.Matrix.for_CommandMatrix;

public class when_captures_of_a_shape_arrive : Specification
{
    CommandMatrix _matrix;

    void Because()
    {
        var lend = Captured.Transaction((ChangeOperation.Insert, "Loans", ["BookId", "MemberId"]), (ChangeOperation.Update, "Inventory", ["AvailableCount"]));
        var lendAndClearReservation = Captured.Transaction((ChangeOperation.Insert, "Loans", ["BookId"]), (ChangeOperation.Update, "Inventory", ["AvailableCount"]), (ChangeOperation.Delete, "Reservations", ["Id"]));
        var twoRows = Captured.Transaction((ChangeOperation.Insert, "Loans", ["BookId"]), (ChangeOperation.Insert, "Loans", ["BookId", "LentOn"]));

        var events = new List<CaptureRecorded>();
        for (var index = 0; index < 5; index++)
        {
            events.Add(Captured.Command("POST", $"/api/loans/{index + 10}/lend", 201, "LoansController.Lend", "Lend a copy", lend));
        }

        events.Add(Captured.Command("POST", "/api/loans/20/lend", 201, "LoansController.Lend", "Lend a copy", lendAndClearReservation));
        events.Add(Captured.Command("POST", "/api/loans/21/lend", 201, "Lending.Handle", "", twoRows));
        events.Add(Captured.Command("POST", "/api/loans/22/lend", 409, "LoansController.Lend"));
        events.Add(Captured.Command("POST", "/api/loans/23/lend", 409, "LoansController.Lend"));
        events.Add(Captured.Command("GET", "/api/books", 200, "BooksController.List"));
        events.Add(RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "TotalCount")));
        _matrix = Captured.Matrix([.. events]);
    }

    ShapeProfile Lend => _matrix.Shapes.Single(profile => profile.Shape.Value == "POST /api/loans/{id}/lend");

    OperationRuns Runs(string table, ChangeOperation operation) =>
        Lend.Writes.Single(write => write.Table.Value == table).Operations.Single(runs => runs.Operation == operation);

    [Fact] void should_keep_one_profile_per_shape() => _matrix.Shapes.Count().ShouldEqual(2);
    [Fact] void should_not_profile_captures_without_a_command() => _matrix.Shapes.Any(profile => profile.Shape.IsNone).ShouldBeFalse();
    [Fact] void should_count_the_captures() => Lend.Captures.ShouldEqual(9);
    [Fact] void should_count_the_captures_that_write_nothing() => Lend.WithoutWrites.ShouldEqual(2);
    [Fact] void should_count_the_runs_of_each_operation() => Runs("Loans", ChangeOperation.Insert).Runs.ShouldEqual(7);
    [Fact] void should_count_an_operation_once_per_run_however_many_rows() => Runs("Loans", ChangeOperation.Insert).Runs.ShouldEqual(Lend.Captures - Lend.WithoutWrites);
    [Fact] void should_count_an_operation_that_only_some_runs_do() => Runs("Reservations", ChangeOperation.Delete).Runs.ShouldEqual(1);
    [Fact] void should_union_the_columns_written() => Lend.Writes.Single(write => write.Table.Value == "Loans").Columns.Select(column => column.Value).ShouldContainOnly(["BookId", "MemberId", "LentOn"]);
    [Fact] void should_keep_the_status_mix_most_common_first() => Lend.Statuses.Select(status => (status.Status.Value, status.Captures)).ShouldContainOnly([(201, 7), (409, 2)]);
    [Fact] void should_remember_which_statuses_write_nothing() => Lend.Statuses.Single(status => status.Status.Value == 409).WithoutWrites.ShouldEqual(2);
    [Fact] void should_name_the_most_common_first_span_first() => Lend.Handlers.First().Span.Value.ShouldEqual("LoansController.Lend");
    [Fact] void should_count_the_handlers() => Lend.Handlers.Select(handler => handler.Captures).ShouldContainOnly([8, 1]);
    [Fact] void should_count_the_captures_per_scenario() => Lend.Scenarios.Single().Captures.ShouldEqual(6);
}
#endif
