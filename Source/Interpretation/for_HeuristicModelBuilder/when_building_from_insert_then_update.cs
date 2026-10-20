// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_HeuristicModelBuilder;

public class when_building_from_insert_then_update : Specification
{
    Capture _capture;
    ExtractionResult _result;

    void Establish() => _capture = new(
        Guid.NewGuid(),
        DateTimeOffset.UnixEpoch,
        [
            new Observation(SourceKind.Http, DateTimeOffset.UnixEpoch, new HttpCommandObserved("POST", "/owners/13/pets/new", 302)),
            new Observation(SourceKind.Postgres, DateTimeOffset.UnixEpoch, new DatabaseTransactionObserved(
                "postgres",
                "petclinic",
                "tx1",
                [
                    new TableChange("public", "pets", ChangeOperation.Insert, ["id", "name"]),
                    new TableChange("public", "pets", ChangeOperation.Update, ["id", "owner_id"])
                ]))
        ]);

    void Because() => _result = new HeuristicModelBuilder().Build(Guid.NewGuid(), [_capture]);

    [Fact] void should_treat_the_update_as_part_of_creation() => Change.Events.Select(@event => @event.Name).ShouldContainOnly(["PetCreated"]);
    [Fact] void should_include_columns_from_the_update_in_creation() => Change.Events.Single(@event => @event.Name == "PetCreated").Properties.Select(property => property.Name).ShouldContainOnly(["Id", "Name", "OwnerId"]);
    [Fact] void should_not_project_a_separate_edit_event() => View.Projections.Single().SourceEvents.ShouldContainOnly(["PetCreated"]);

    ExtractedSlice Change => _result.Modules.Single().Features.Single().Slices.Single(slice => slice.Type == ExtractedSliceType.StateChange);
    ExtractedSlice View => _result.Modules.Single().Features.Single().Slices.Single(slice => slice.Type == ExtractedSliceType.StateView);
}
#endif
