// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_ModelRenamer;

public class when_applying_a_refinement : Specification
{
    ExtractionResult _model;
    ModelRefinement _refinement;
    ExtractionResult _result;

    void Establish()
    {
        _model = new ExtractionResult(
            Guid.NewGuid(),
            [
                new ExtractedModule(
                    "Api",
                    [
                        new ExtractedFeature(
                            "Authors",
                            [new ExtractedFeature("Profiles", [], [Slice("Describe", new ExtractedCommand("DescribeAuthor", [], []))])],
                            [
                                new ExtractedSlice(
                                    "Create",
                                    ExtractedSliceType.StateChange,
                                    [new ExtractedCommand("CreateAuthor", [new ExtractedProperty("Name", "string")], [])],
                                    [new ExtractedEvent("AuthorCreated", [new ExtractedProperty("Name", "string")])],
                                    [],
                                    [new ExtractedProjection("AuthorProjection", ["AuthorCreated"])],
                                    [])
                            ])
                    ])
            ]);

        // Description keys refer to the RENAMED names — the module is renamed to Library, so every description
        // key uses Library, not Api.
        _refinement = new ModelRefinement(
            "LibrarySystem",
            new Dictionary<string, string>
            {
                ["Api"] = "Library",
                ["CreateAuthor"] = "RegisterAuthor",
                ["AuthorCreated"] = "AuthorRegistered"
            },
            new Dictionary<string, string>
            {
                ["module:Library"] = "Everything about the library",
                ["feature:Library/Authors"] = "The author lifecycle",
                ["feature:Library/Authors/Profiles"] = "Author profile curation",
                ["slice:Library/Authors/Create"] = "Registers an author",
                ["slice:Library/Authors/Profiles/Describe"] = "Describes an author profile",
                ["command:Library/Authors/Create/RegisterAuthor"] = "Registers an author with their name and produces the registration fact",
                ["command:Library/Authors/Profiles/Describe/DescribeAuthor"] = "Captures the description of an author profile",
                ["slice:Library/Unknown/Nope"] = "Should be ignored"
            },
            []);
    }

    void Because() => _result = ModelRenamer.Apply(_model, _refinement);

    [Fact] void should_apply_the_system_name() => _result.SystemName.ShouldEqual("LibrarySystem");
    [Fact] void should_rename_the_module() => Module.Name.ShouldEqual("Library");
    [Fact] void should_rename_the_command() => Feature.Slices.Single().Commands.Single().Name.ShouldEqual("RegisterAuthor");
    [Fact] void should_rename_the_event_reference_in_the_projection() => Feature.Slices.Single().Projections.Single().SourceEvents.Single().ShouldEqual("AuthorRegistered");
    [Fact] void should_describe_the_module_by_its_renamed_key() => Module.Description.ShouldEqual("Everything about the library");
    [Fact] void should_describe_the_feature() => Feature.Description.ShouldEqual("The author lifecycle");
    [Fact] void should_describe_the_sub_feature() => Feature.SubFeatures.Single().Description.ShouldEqual("Author profile curation");
    [Fact] void should_describe_the_slice() => Feature.Slices.Single().Description.ShouldEqual("Registers an author");
    [Fact] void should_describe_the_sub_feature_slice() => Feature.SubFeatures.Single().Slices.Single().Description.ShouldEqual("Describes an author profile");
    [Fact] void should_describe_the_command_by_its_renamed_key() => Feature.Slices.Single().Commands.Single().Description.ShouldEqual("Registers an author with their name and produces the registration fact");
    [Fact] void should_describe_the_sub_feature_slice_command() => Feature.SubFeatures.Single().Slices.Single().Commands.Single().Description.ShouldEqual("Captures the description of an author profile");

    ExtractedModule Module => _result.Modules.Single();

    ExtractedFeature Feature => Module.Features.Single();

    static ExtractedSlice Slice(string name, params ExtractedCommand[] commands) => new(name, ExtractedSliceType.StateChange, commands, [], [], [], []);
}
#endif
