// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Screenplay.Syntax;

namespace Cratis.Prologue.Screenplay.for_ScreenplayGenerator;

public class when_generating_required_numeric_properties : given.a_library_extraction_result
{
    CommandSyntax _command;

    void Establish()
    {
        var slice = _result.Modules.Single().Features.Single().Slices[0];
        var command = slice.Commands.Single();
        _result = _result with
        {
            Modules = [_result.Modules.Single() with
            {
                Features = [_result.Modules.Single().Features.Single() with
                {
                    Slices = [slice with
                    {
                        Commands = [command with
                        {
                            Properties = [.. command.Properties, new("TypeId", "int", IsRequired: true)],
                            Validations = [.. command.Validations, new("TypeId", ExtractedValidationRuleKind.Required, string.Empty, "TypeId is required")]
                        }]
                    }]
                }]
            }]
        };
    }

    void Because() => _command = _generator.Build(_result).Modules.Single().Features.Single().Slices.Single().Commands.Single();

    [Fact] void should_not_emit_a_text_rule_for_a_required_integer() => _command.Validations.OfType<DeclarativeValidateSyntax>().SelectMany(validation => validation.Rules).Any(rule => rule.Property == "typeId").ShouldBeFalse();
    [Fact] void should_preserve_required_text_validation() => _command.Validations.OfType<DeclarativeValidateSyntax>().SelectMany(validation => validation.Rules).Any(rule => rule.Property == "name" && rule.Rule == ValidationRuleKind.NotEmpty).ShouldBeTrue();
}
#endif
