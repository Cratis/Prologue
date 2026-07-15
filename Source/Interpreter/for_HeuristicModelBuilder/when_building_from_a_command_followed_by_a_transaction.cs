// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter.for_HeuristicModelBuilder;

public class when_building_from_a_command_followed_by_a_transaction : Specification
{
    static readonly Guid _prologueId = Guid.NewGuid();
    static readonly DateTimeOffset _occurred = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    HeuristicModelBuilder _builder;
    ExtractionResult _result;
    ExtractedModule _module;

    void Establish()
    {
        _builder = new HeuristicModelBuilder();

        var capture = new Capture(
            Guid.NewGuid(),
            _occurred,
            [
                new Observation(SourceKind.Http, _occurred, new HttpCommandObserved("POST", "/api/authors/register", 201)),
                new Observation(SourceKind.SqlServer, _occurred.AddMilliseconds(20), new DatabaseTransactionObserved(
                    "sqlserver", "Shop", "tx1", [new TableChange("dbo", "Authors", ChangeOperation.Insert, ["AuthorId", "Name"])]))
            ],
            _prologueId);

        _result = _builder.Build(_prologueId, [capture]);
    }

    void Because() => _module = _result.Modules.Single();

    [Fact] void should_carry_the_prologue_id() => _result.PrologueId.ShouldEqual(_prologueId);
    [Fact] void should_derive_the_module_from_the_path() => _module.Name.ShouldEqual("Authors");

    [Fact]
    void should_produce_a_state_change_slice() =>
        _module.Features.Single().Slices.Count(slice => slice.Type == ExtractedSliceType.StateChange).ShouldEqual(1);

    [Fact]
    void should_produce_a_state_view_slice() =>
        _module.Features.Single().Slices.Count(slice => slice.Type == ExtractedSliceType.StateView).ShouldEqual(1);

    [Fact]
    void should_name_the_command_from_the_action_and_resource() =>
        StateChange.Commands.Single().Name.ShouldEqual("RegisterAuthor");

    [Fact]
    void should_name_the_slice_after_the_action() =>
        StateChange.Name.ShouldEqual("Register");

    [Fact]
    void should_derive_a_past_tense_event_from_the_inserted_table() =>
        StateChange.Events.Single().Name.ShouldEqual("AuthorCreated");

    [Fact]
    void should_infer_the_event_properties_from_the_changed_columns() =>
        StateChange.Events.Single().Properties.Select(property => property.Name).ShouldContainOnly(["AuthorId", "Name"]);

    [Fact]
    void should_build_a_read_model_for_the_entity() =>
        StateView.ReadModels.Single().Name.ShouldEqual("Author");

    [Fact]
    void should_point_the_projection_at_the_event() =>
        StateView.Projections.Single().SourceEvents.ShouldContain("AuthorCreated");

    ExtractedSlice StateChange => _module.Features.Single().Slices.Single(slice => slice.Type == ExtractedSliceType.StateChange);

    ExtractedSlice StateView => _module.Features.Single().Slices.Single(slice => slice.Type == ExtractedSliceType.StateView);
}
#endif
