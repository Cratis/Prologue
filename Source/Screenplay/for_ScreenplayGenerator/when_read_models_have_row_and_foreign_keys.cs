// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Screenplay.Syntax;

namespace Cratis.Prologue.Screenplay.for_ScreenplayGenerator;

public class when_read_models_have_row_and_foreign_keys : given.a_library_extraction_result
{
    IReadOnlyList<ReadModelSyntax> _readModels;

    void Establish()
    {
        var module = _result.Modules.Single();
        var feature = module.Features.Single();
        var slice = feature.Slices[^1];
        _result = _result with
        {
            Modules = [module with
            {
                Features = [feature with
                {
                    Slices = [slice with
                    {
                        ReadModels =
                        [
                            new("Pet", [new("TypeId", "int"), new("Id", "int"), new("OwnerId", "int")]),
                            new("Owner", [new("Id", "int"), new("Name", "string")]),
                            new("Visit", [new("PetId", "int"), new("Id", "int")])
                        ],
                        Projections = []
                    }]
                }]
            }]
        };
    }

    void Because() => _readModels = [.. _generator.Build(_result).Modules.Single().Features.Single().Slices.Single().ReadModels ?? []];

    [Fact] void should_key_pets_only_by_the_row_identity() => KeysOf("Pet").ShouldContainOnly(["id"]);
    [Fact] void should_key_owners_by_the_row_identity() => KeysOf("Owner").ShouldContainOnly(["id"]);
    [Fact] void should_not_use_the_pet_foreign_key_as_the_visit_identity() => KeysOf("Visit").ShouldContainOnly(["id"]);

    IEnumerable<string> KeysOf(string model) => _readModels.Single(readModel => readModel.Name == model).Properties.Where(property => property.IsKey).Select(property => property.Name);
}
#endif
