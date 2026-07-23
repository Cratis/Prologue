// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_HeuristicModelBuilder;

public class when_building_from_a_delete_command : Specification
{
    static readonly Guid _prologueId = Guid.NewGuid();
    static readonly DateTimeOffset _occurred = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    HeuristicModelBuilder _builder;
    ExtractedSlice _stateChange;

    void Establish()
    {
        _builder = new HeuristicModelBuilder();

        // A real capture of DELETE /api/authors/{id}: the concrete request path carries the id, but the correlated
        // server span carries the templated route as http.route — which is what should drive the name.
        var capture = new Capture(
            Guid.NewGuid(),
            _occurred,
            [
                new Observation(SourceKind.Http, _occurred, new HttpCommandObserved("DELETE", "/api/authors/00000802-4a1b-5c6d-7e8f-90a1b2c3d4e5", 200)),
                new Observation(SourceKind.OpenTelemetry, _occurred.AddMilliseconds(10), new TelemetryObserved(
                    "trace1",
                    "span1",
                    string.Empty,
                    "DELETE /api/authors/{id}",
                    "Server",
                    "LibraryApi",
                    1,
                    12,
                    ["http.request.method", "http.route"],
                    new Dictionary<string, string> { ["http.request.method"] = "DELETE", ["http.route"] = "/api/authors/{id}" })),
                new Observation(SourceKind.SqlServer, _occurred.AddMilliseconds(20), new DatabaseTransactionObserved(
                    "sqlserver", "LibraryDb", "tx1", [new TableChange("dbo", "Authors", ChangeOperation.Delete, ["AuthorId"])]))
            ],
            _prologueId);

        var result = _builder.Build(_prologueId, [capture]);
        _stateChange = result.Modules.Single().Features.Single().Slices.Single(slice => slice.Type == ExtractedSliceType.StateChange);
    }

    void Because() { }

    [Fact]
    void should_name_the_command_from_the_method_and_resource_without_the_id() =>
        _stateChange.Commands.Single().Name.ShouldEqual("DeleteAuthor");

    [Fact]
    void should_not_bleed_the_path_or_id_into_the_command_name() =>
        _stateChange.Commands.Single().Name.Contains("Api").ShouldBeFalse();

    [Fact]
    void should_name_the_slice_after_the_delete_action() =>
        _stateChange.Name.ShouldEqual("Delete");

    [Fact]
    void should_derive_a_past_tense_delete_event() =>
        _stateChange.Events.Single().Name.ShouldEqual("AuthorDeleted");
}
#endif
