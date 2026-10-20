// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Screenplay.Syntax;

namespace Cratis.Prologue.Screenplay.for_ScreenplayGenerator;

public class when_declaring_read_models : given.a_library_extraction_result
{
    SliceSyntax _slice;

    void Because() => _slice = _generator.Build(_result).Modules.Single().Features.Single().Slices.Last();

    [Fact] void should_declare_the_queried_read_model() => (_slice.ReadModels ?? []).Single().Name.ShouldEqual("Author");
    [Fact] void should_declare_the_read_model_properties() => (_slice.ReadModels ?? []).Single().Properties.Select(property => property.Name).ShouldContainOnly(["authorId", "name", "email"]);
    [Fact] void should_explicitly_key_the_read_model_on_its_identity() => (_slice.ReadModels ?? []).Single().Properties.Where(property => property.IsKey).Select(property => property.Name).ShouldContainOnly(["authorId"]);
}
#endif
