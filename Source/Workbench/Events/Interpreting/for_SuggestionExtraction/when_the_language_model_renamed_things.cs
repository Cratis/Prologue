// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_SuggestionExtraction;

public class when_the_language_model_renamed_things : a_library_interpretation
{
    IReadOnlyList<SuggestedEvent> _refined;
    InterpretedModel _refinedModel;
    ExtractionResult _final;

    void Because()
    {
        var renames = new Dictionary<string, string> { ["AuthorCreated"] = "AuthorRegistered", ["Authors"] = "People", ["RegisterAuthor"] = "EnrollAuthor" };
        _final = ModelRenamer.Apply(_heuristic, renames);
        (_refined, _refinedModel) = SuggestionExtraction.From(_partitions, _heuristic, _final with { SystemName = "Library" }, renames, _facts);
    }

    [Fact] void should_keep_the_key_of_the_mechanical_name() => _refined.Select(suggestion => suggestion.Key).ShouldEqual(_suggestions.Select(suggestion => suggestion.Key));
    [Fact] void should_show_the_refined_name() => _refined.Single(suggestion => suggestion.Key == Key("Authors", "AuthorCreated")).Refined.Value.ShouldEqual("AuthorRegistered");
    [Fact] void should_keep_the_mechanical_name_beside_it() => _refined.Single(suggestion => suggestion.Key == Key("Authors", "AuthorCreated")).Mechanical.Value.ShouldEqual("AuthorCreated");
    [Fact] void should_show_the_refined_module() => _refined.Single(suggestion => suggestion.Key == Key("Authors", "AuthorCreated")).Module.Value.ShouldEqual("People");
    [Fact] void should_use_the_refined_names_in_the_structure() => _refinedModel.Slices.SelectMany(slice => slice.Commands).Select(command => command.Name.Value).ShouldContain("EnrollAuthor");
    [Fact] void should_keep_the_system_name() => _refinedModel.SystemName.ShouldEqual("Library");
}
#endif
