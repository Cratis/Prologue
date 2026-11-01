// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Rebuild.Drift;

namespace Cratis.Prologue.Workbench.Rebuild.for_ModelDrift;

public class when_captures_arrive_after_the_model_is_accepted : Specification
{
    EventContext _context;
    ModelDrift _beforeAcceptance;
    ModelDrift _drift;

    void Establish() => _context = EventContext.EmptyWithEventSourceId(PrologueId.New());

    void Because()
    {
        var drift = Reduce(null, RecordedCaptures.Command("POST", "/api/loans", 201, [RecordedCaptures.Write(ChangeOperation.Insert, "Loans", "BookId", "MemberId")]));
        drift = Reduce(drift, RecordedCaptures.Command("PUT", "/api/members/12", 200, [RecordedCaptures.Write(ChangeOperation.Update, "Members", "FirstName")]));
        drift = Reduce(drift, RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "AvailableCount")));
        _beforeAcceptance = drift;

        drift = ModelDrift.Reduce(new ModelAcceptedForRebuild(), drift, _context);
        drift = Reduce(drift, RecordedCaptures.Command("POST", "/api/loans", 201, [RecordedCaptures.Write(ChangeOperation.Insert, "Loans", "BookId")]));
        drift = Reduce(drift, RecordedCaptures.Command("PUT", "/api/members/77", 200, [RecordedCaptures.Write(ChangeOperation.Update, "Members", "FirstName", "LastName")]));
        drift = Reduce(drift, RecordedCaptures.Command("PUT", "/api/members/78", 200, [RecordedCaptures.Write(ChangeOperation.Update, "Members", "LastName")]));
        drift = Reduce(drift, RecordedCaptures.Command("DELETE", "/api/reservations/5", 204));
        drift = Reduce(drift, RecordedCaptures.Command("DELETE", "/api/reservations/6", 204));
        drift = Reduce(drift, RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "Stock")));
        _drift = drift;
    }

    [Fact] void should_learn_the_shapes_before_the_model_is_accepted() => _beforeAcceptance.Known.Select(known => known.Shape.Value).ShouldContainOnly(["POST /api/loans", "PUT /api/members/{id}"]);
    [Fact] void should_not_report_drift_before_the_model_is_accepted() => _beforeAcceptance.Items.ShouldBeEmpty();
    [Fact] void should_not_report_a_known_shape_writing_what_it_always_wrote() => _drift.Items.Any(item => item.Shape.Value == "POST /api/loans").ShouldBeFalse();
    [Fact] void should_report_a_column_a_known_shape_newly_writes() => _drift.Items.Single(item => item.Kind == DriftKind.NewColumn).Column.Value.ShouldEqual("dbo.Members.LastName");
    [Fact] void should_count_how_often_a_new_column_was_seen() => _drift.Items.Single(item => item.Kind == DriftKind.NewColumn).Captures.ShouldEqual(2);
    [Fact] void should_report_a_shape_first_seen_after_acceptance() => _drift.Items.Single(item => item.Kind == DriftKind.NewShape).Shape.Value.ShouldEqual("DELETE /api/reservations/{id}");
    [Fact] void should_count_how_often_a_new_shape_was_seen() => _drift.Items.Single(item => item.Kind == DriftKind.NewShape).Captures.ShouldEqual(2);
    [Fact] void should_ignore_database_changes_with_no_command() => _drift.Items.Count().ShouldEqual(2);

    ModelDrift Reduce(ModelDrift? current, CaptureRecorded capture) => ModelDrift.Reduce(capture, current, _context);
}
#endif
