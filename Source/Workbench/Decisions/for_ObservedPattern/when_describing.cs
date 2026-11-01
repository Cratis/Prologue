// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Live;

namespace Cratis.Prologue.Workbench.Decisions.for_ObservedPattern;

public class when_describing : Specification
{
    static RecordedCapture Of(int status, params RecordedTransaction[] writes) =>
        RecordedCapture.From(RecordedCaptures.Command("POST", "/api/loans", status, writes));

    [Fact] void should_say_nothing_was_written_for_a_refusal_without_writes() => ObservedPattern.Describe(Of(409)).Value.ShouldEqual("nothing was written (409)");
    [Fact] void should_name_what_was_written_for_an_acceptance() =>
        ObservedPattern.Describe(Of(201, RecordedCaptures.Write(ChangeOperation.Insert, "Loans"), RecordedCaptures.Write(ChangeOperation.Update, "Inventory"))).Value.ShouldEqual("it wrote INSERT Loans, UPDATE Inventory");
    [Fact] void should_name_the_writes_of_a_refusal_that_wrote_something() =>
        ObservedPattern.Describe(Of(500, RecordedCaptures.Write(ChangeOperation.Insert, "Loans"))).Value.ShouldEqual("it answered 500 although it wrote INSERT Loans");
    [Fact] void should_say_so_for_an_acceptance_without_writes() => ObservedPattern.Describe(Of(200)).Value.ShouldEqual("nothing needed to be written");
}
#endif
