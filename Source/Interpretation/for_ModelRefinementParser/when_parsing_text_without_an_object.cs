// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpretation.for_ModelRefinementParser;

public class when_parsing_text_without_an_object : Specification
{
    ModelRefinement? _refinement;

    void Because() => _refinement = ModelRefinementParser.Parse("I could not produce a refinement for this model.");

    [Fact] void should_yield_no_refinement() => _refinement.ShouldBeNull();
}
#endif
