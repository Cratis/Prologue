// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_ModelRenamer;

public class when_applying_a_rename_map : Specification
{
    ExtractionResult _result;
    ExtractedSlice _slice;

    void Establish()
    {
        var command = new ExtractedCommand("CreateAuthor", [new ExtractedProperty("AuthorId", "Guid")], []);
        var @event = new ExtractedEvent("AuthorCreated", [new ExtractedProperty("AuthorId", "Guid")]);
        var projection = new ExtractedProjection("AuthorProjection", ["AuthorCreated"]);
        var constraint = new ExtractedConstraint("UniqueEmail", "email", "AuthorCreated");
        var slice = new ExtractedSlice("Create", ExtractedSliceType.StateChange, [command], [@event], [], [projection], [constraint]);
        var feature = new ExtractedFeature("Authors", [], [slice]);
        var source = new ExtractionResult(Guid.NewGuid(), [new ExtractedModule("Authors", [feature])]);

        var renames = new Dictionary<string, string>
        {
            ["Create"] = "Register",
            ["CreateAuthor"] = "RegisterAuthor",
            ["AuthorCreated"] = "AuthorRegistered"
        };

        _result = ModelRenamer.Apply(source, renames);
    }

    void Because() => _slice = _result.Modules.Single().Features.Single().Slices.Single();

    [Fact] void should_rename_the_slice() => _slice.Name.ShouldEqual("Register");
    [Fact] void should_rename_the_command() => _slice.Commands.Single().Name.ShouldEqual("RegisterAuthor");
    [Fact] void should_rename_the_event() => _slice.Events.Single().Name.ShouldEqual("AuthorRegistered");
    [Fact] void should_rewrite_the_projection_source_event() => _slice.Projections.Single().SourceEvents.ShouldContain("AuthorRegistered");
    [Fact] void should_rewrite_the_constraint_event_reference() => _slice.Constraints.Single().OnEvent.ShouldEqual("AuthorRegistered");
    [Fact] void should_keep_names_absent_from_the_map() => _slice.Commands.Single().Properties.Single().Name.ShouldEqual("AuthorId");
}
#endif
