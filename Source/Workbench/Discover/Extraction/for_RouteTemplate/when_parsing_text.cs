// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Discover.Extraction.for_RouteTemplate;

public class when_parsing_text : Specification
{
    RouteShape? _shape;
    RouteShape? _notAShape;
    RouteShape? _noPath;

    void Because()
    {
        _shape = RouteTemplate.Parse(" post /api/loans/12/return ");
        _notAShape = RouteTemplate.Parse("loans return");
        _noPath = RouteTemplate.Parse("POST");
    }

    [Fact] void should_normalize_the_method_and_path() => _shape!.Value.ShouldEqual("POST /api/loans/{id}/return");
    [Fact] void should_refuse_text_that_is_not_a_method_and_a_path() => _notAShape.ShouldBeNull();
    [Fact] void should_refuse_a_method_without_a_path() => _noPath.ShouldBeNull();
}
#endif
