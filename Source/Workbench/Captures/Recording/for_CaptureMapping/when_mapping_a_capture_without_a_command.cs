// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Captures.Recording.for_CaptureMapping;

public class when_mapping_a_capture_without_a_command : Specification
{
    RecordCapture _command;

    void Because()
    {
        var at = DateTimeOffset.UtcNow;
        var capture = new Capture(
            Guid.NewGuid(),
            at,
            [new(SourceKind.Postgres, at, new DatabaseTransactionObserved("PostgreSQL", "library", "tx1", [new TableChange("public", "Inventory", ChangeOperation.Update, ["TotalCount"])]))]);

        _command = CaptureMapping.ToCommand(capture);
    }

    [Fact] void should_record_an_empty_command() => _command.Http.ShouldEqual(RecordedCommand.None);
    [Fact] void should_have_no_shape() => _command.Http.Shape.IsNone.ShouldBeTrue();
    [Fact] void should_record_for_the_unassigned_prologue() => _command.Prologue.ShouldEqual(PrologueId.Unassigned);
    [Fact] void should_keep_the_transaction() => _command.Transactions.Count().ShouldEqual(1);
    [Fact] void should_not_be_trace_linked() => _command.TraceLinked.ShouldBeFalse();
}
#endif
