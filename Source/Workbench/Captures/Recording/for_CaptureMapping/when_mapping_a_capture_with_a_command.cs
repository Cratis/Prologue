// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG

namespace Cratis.Prologue.Workbench.Captures.Recording.for_CaptureMapping;

public class when_mapping_a_capture_with_a_command : Specification
{
    static readonly DateTimeOffset _start = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);
    Guid _prologue;
    RecordCapture _command;

    void Establish()
    {
        _prologue = Guid.NewGuid();
    }

    void Because()
    {
        var capture = new Capture(
            Guid.NewGuid(),
            _start,
            [
                new(SourceKind.OpenTelemetry, _start.AddMilliseconds(40), new TelemetryObserved("trace-1", "s1", string.Empty, "LoansController.Lend", "Server", "Lending", 0, 90, [], new Dictionary<string, string>())),
                new(SourceKind.Http, _start.AddMilliseconds(10), new HttpCommandObserved("post", "/api/loans/2649/return?x=1", 200, "trace-1")),
                new(SourceKind.Postgres, _start.AddMilliseconds(130), new DatabaseTransactionObserved("PostgreSQL", "library", "tx1", [new TableChange("public", "Loans", ChangeOperation.Update, ["ReturnedOn"])]))
            ],
            _prologue)
        {
            Service = new ServiceName("Lending")
        };

        _command = CaptureMapping.ToCommand(capture);
    }

    [Fact] void should_record_for_the_capture_prologue() => _command.Prologue.Value.ShouldEqual(_prologue);
    [Fact] void should_normalize_the_command_shape() => _command.Http.Shape.Value.ShouldEqual("POST /api/loans/{id}/return");
    [Fact] void should_drop_the_query_string_from_the_path() => _command.Http.Path.Value.ShouldEqual("/api/loans/2649/return");
    [Fact] void should_keep_the_status() => _command.Http.Status.Value.ShouldEqual(200);
    [Fact] void should_place_the_command_relative_to_the_first_observation() => _command.Http.OffsetMilliseconds.ShouldEqual(0);
    [Fact] void should_keep_the_columns_of_the_transaction() => _command.Transactions.Single().Tables.Single().Columns.Select(column => column.Value).ShouldContainOnly(["ReturnedOn"]);
    [Fact] void should_place_the_transaction_in_time() => _command.Transactions.Single().OffsetMilliseconds.ShouldEqual(120);
    [Fact] void should_count_the_spans() => _command.SpanCount.ShouldEqual(1);
    [Fact] void should_link_the_spans_by_trace() => _command.TraceLinked.ShouldBeTrue();
    [Fact] void should_keep_the_service() => _command.Service.Value.ShouldEqual("Lending");
}
#endif
