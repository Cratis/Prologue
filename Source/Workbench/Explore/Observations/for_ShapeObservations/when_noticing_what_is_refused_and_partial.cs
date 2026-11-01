// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Explore.given;
using Cratis.Prologue.Workbench.Explore.Matrix;

namespace Cratis.Prologue.Workbench.Explore.Observations.for_ShapeObservations;

public class when_noticing_what_is_refused_and_partial : Specification
{
    ShapeProfile _lend;
    ShapeProfile _steady;

    void Because()
    {
        var lend = Captured.Transaction((ChangeOperation.Insert, "Loans", ["BookId"]), (ChangeOperation.Update, "Inventory", ["AvailableCount"]));
        var clearing = Captured.Transaction((ChangeOperation.Insert, "Loans", ["BookId"]), (ChangeOperation.Delete, "Reservations", ["Id"]));
        var captures = new List<CaptureRecorded>();
        for (var index = 0; index < 8; index++)
        {
            captures.Add(Captured.Command("POST", "/api/loans", 201, "", "", index < 2 ? clearing : lend));
        }

        captures.Add(Captured.Command("POST", "/api/loans", 409));
        captures.Add(Captured.Command("POST", "/api/loans", 409));
        _lend = Captured.Matrix([.. captures]).Shapes.Single();

        var update = Captured.Transaction((ChangeOperation.Update, "Loans", ["ReturnedOn"]));
        _steady = Captured.Matrix(
            Captured.Command("POST", "/api/loans/1/return", 200, "", "", update),
            Captured.Command("POST", "/api/loans/2/return", 200, "", "", update)).Shapes.Single();
    }

    string Text(ObservationKind kind) => _lend.Observations.Single(observation => observation.Kind == kind).Text;

    [Fact] void should_say_how_many_answer_a_refusal_and_write_nothing() => Text(ObservationKind.AnswersWithoutWriting).ShouldEqual("20% of runs answer 409 and write nothing.");
    [Fact] void should_say_which_writes_happen_only_in_some_runs() => _lend.Observations.Any(observation => observation.Text == "Writes Reservations only in some runs: delete in 20% of runs.").ShouldBeTrue();
    [Fact] void should_not_call_a_write_partial_when_every_writing_run_does_it() => _lend.Observations.Any(observation => observation.Text.StartsWith("Writes Loans", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_have_nothing_to_say_about_a_steady_shape() => _steady.Observations.ShouldBeEmpty();
}
#endif
