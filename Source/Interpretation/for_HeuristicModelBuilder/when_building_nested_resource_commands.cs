// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_HeuristicModelBuilder;

public class when_building_nested_resource_commands : Specification
{
    ExtractionResult _result;
    IReadOnlyList<Capture> _captures;

    void Establish() => _captures =
    [
        CaptureFor("/owners/new", "owners", ChangeOperation.Insert),
        CaptureFor("/owners/13/pets/new", "pets", ChangeOperation.Insert),
        CaptureFor("/owners/14/pets/new", "pets", ChangeOperation.Insert),
        CaptureFor("/owners/13/pets/15/visits/new", "visits", ChangeOperation.Insert),
        CaptureFor("/owners/13/edit", "owners", ChangeOperation.Update),
        CaptureFor("/owners/13/pets/15/edit", "pets", ChangeOperation.Update)
    ];

    void Because() => _result = new HeuristicModelBuilder().Build(Guid.NewGuid(), _captures);

    [Fact] void should_only_associate_owner_creation_with_its_own_event() => EventsFor("NewOwner").ShouldContainOnly(["OwnerCreated"]);
    [Fact] void should_only_associate_pet_creation_with_its_own_event() => EventsFor("NewPet").ShouldContainOnly(["PetCreated"]);
    [Fact] void should_only_associate_visit_creation_with_its_own_event() => EventsFor("NewVisit").ShouldContainOnly(["VisitCreated"]);
    [Fact] void should_only_associate_owner_edits_with_their_own_event() => EventsFor("EditOwner").ShouldContainOnly(["OwnerUpdated"]);
    [Fact] void should_only_associate_pet_edits_with_their_own_event() => EventsFor("EditPet").ShouldContainOnly(["PetUpdated"]);
    [Fact] void should_merge_repeated_captures_of_the_same_command() => Changes.Count(slice => slice.Commands.Any(command => command.Name == "NewPet")).ShouldEqual(1);
    [Fact] void should_keep_slice_names_unique() => Changes.Select(slice => slice.Name).Distinct().Count().ShouldEqual(Changes.Count());

    IEnumerable<ExtractedSlice> Changes => _result.Modules.Single().Features.Single().Slices.Where(slice => slice.Type == ExtractedSliceType.StateChange);

    IEnumerable<string> EventsFor(string command) => Changes.Single(slice => slice.Commands.Any(candidate => candidate.Name == command)).Events.Select(@event => @event.Name);

    static Capture CaptureFor(string path, string table, ChangeOperation operation) => new(
        Guid.NewGuid(),
        DateTimeOffset.UnixEpoch,
        [
            new Observation(SourceKind.Http, DateTimeOffset.UnixEpoch, new HttpCommandObserved("POST", path, 302)),
            new Observation(SourceKind.Postgres, DateTimeOffset.UnixEpoch, new DatabaseTransactionObserved(
                "postgres", "petclinic", Guid.NewGuid().ToString(), [new TableChange("public", table, operation, ["id", "name"])]))
        ]);
}
#endif
