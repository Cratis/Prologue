// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Screenplay.Syntax;

namespace Cratis.Prologue.Screenplay.for_ScreenplayGenerator;

public class when_an_event_is_shared_between_slices : given.a_library_extraction_result
{
    ApplicationSyntax _application;

    void Establish()
    {
        var module = _result.Modules.Single();
        var feature = module.Features.Single();
        var slice = feature.Slices[0];
        _result = _result with
        {
            Modules = [module with
            {
                Features = [feature with
                {
                    SubFeatures = [new("OtherAuthors", [], [slice with { Name = "Import", Commands = [slice.Commands.Single() with { Name = "ImportAuthor" }] }])]
                }]
            }]
        };
    }

    void Because() => _application = _generator.Build(_result);

    [Fact] void should_declare_the_event_only_once_across_nested_features() => Slices.SelectMany(slice => slice.Events).Count(@event => @event.Name == "AuthorRegistered").ShouldEqual(1);
    [Fact] void should_preserve_both_commands_producing_the_event() => Slices.SelectMany(slice => slice.Commands).Sum(command => command.Produces.Count()).ShouldEqual(2);

    IEnumerable<SliceSyntax> Slices => _application.Modules.Single().Features.Single().Slices.Concat(_application.Modules.Single().Features.Single().Features.Single().Slices);
}
#endif
