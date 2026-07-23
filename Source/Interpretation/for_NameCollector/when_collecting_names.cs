// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_NameCollector;

public class when_collecting_names : Specification
{
    IReadOnlyList<string> _names;

    void Because()
    {
        var command = new ExtractedCommand("CreateAuthor", [new ExtractedProperty("AuthorId", "Guid")], []);
        var @event = new ExtractedEvent("AuthorCreated", [new ExtractedProperty("Name", "string")]);
        var readModel = new ExtractedReadModel("Author", [new ExtractedProperty("Name", "string")]);
        var slice = new ExtractedSlice("Create", ExtractedSliceType.StateChange, [command], [@event], [readModel], [], []);
        var feature = new ExtractedFeature("Registration", [], [slice]);
        var result = new ExtractionResult(Guid.NewGuid(), [new ExtractedModule("Authors", [feature])]);

        _names = NameCollector.Collect(result);
    }

    [Fact] void should_collect_the_module_name() => _names.ShouldContain("Authors");
    [Fact] void should_collect_the_feature_name() => _names.ShouldContain("Registration");
    [Fact] void should_collect_the_slice_name() => _names.ShouldContain("Create");
    [Fact] void should_collect_the_command_name() => _names.ShouldContain("CreateAuthor");
    [Fact] void should_collect_the_event_name() => _names.ShouldContain("AuthorCreated");
    [Fact] void should_collect_the_read_model_name() => _names.ShouldContain("Author");
    [Fact] void should_collect_property_names() => _names.ShouldContain("AuthorId");
    [Fact] void should_deduplicate_repeated_names() => _names.Count(name => name == "Name").ShouldEqual(1);
}
#endif
