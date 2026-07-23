// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_SystemNameDeriver;

public class when_deriving_from_modules : Specification
{
    ExtractionResult _model;
    string _name;

    void Establish() => _model = new ExtractionResult(
        Guid.NewGuid(),
        [
            new ExtractedModule("Admin", [new ExtractedFeature("Users", [], [Slice("Invite")])]),
            new ExtractedModule(
                "Library",
                [
                    new ExtractedFeature("Authors", [Nested("Profiles", Slice("Describe"))], [Slice("Register")]),
                    new ExtractedFeature("Books", [], [Slice("Catalog")])
                ])
        ]);

    void Because() => _name = SystemNameDeriver.Derive(_model);

    [Fact] void should_pick_the_module_with_the_most_slices() => _name.ShouldEqual("Library");

    static ExtractedSlice Slice(string name) => new(name, ExtractedSliceType.StateChange, [], [], [], [], []);

    static ExtractedFeature Nested(string name, ExtractedSlice slice) => new(name, [], [slice]);
}
#endif
