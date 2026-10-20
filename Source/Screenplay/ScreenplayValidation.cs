// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Screenplay.Diagnostics;
using Cratis.Screenplay.Syntax;

namespace Cratis.Prologue.Screenplay;

/// <summary>
/// Maps inferred validation rules to rules admitted for the property's Screenplay type. Required numeric values
/// are represented by non-optional properties, not text-only <c language="csharp">not empty</c> rules.
/// </summary>
internal static class ScreenplayValidation
{
    internal static IEnumerable<ValidateSyntax> From(ExtractedCommand command)
    {
        var rules = command.Validations
            .Where(rule => rule.Kind != ExtractedValidationRuleKind.Required || command.Properties.Any(property =>
                property.Name == rule.Property && ScreenplayTypeMapping.TypeFor(property.Type) == "String"))
            .Select(Rule)
            .OfType<ValidationRuleSyntax>()
            .ToList();
        return rules.Count == 0 ? [] : [new DeclarativeValidateSyntax(rules, SourceLocation.Start)];
    }

    static ValidationRuleSyntax? Rule(ExtractedValidationRule rule)
    {
        var property = Camelize(rule.Property);
        var message = rule.Message.Length == 0 ? null : rule.Message;
        return rule.Kind switch
        {
            ExtractedValidationRuleKind.Required =>
                new(property, ValidationRuleKind.NotEmpty, null, message, SourceLocation.Start),
            ExtractedValidationRuleKind.MaxLength when TryNumber(rule.Argument, out var maximum) =>
                new(property, ValidationRuleKind.Max, Literal(maximum), message, SourceLocation.Start),
            ExtractedValidationRuleKind.MinLength when TryNumber(rule.Argument, out var minimum) =>
                new(property, ValidationRuleKind.Min, Literal(minimum), message, SourceLocation.Start),
            ExtractedValidationRuleKind.Pattern when rule.Argument.Length > 0 =>
                new(property, ValidationRuleKind.Matches, Literal(rule.Argument), message, SourceLocation.Start),
            _ => null
        };
    }

    static string Camelize(string name) =>
        name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name[1..];

    static bool TryNumber(string argument, out int number) =>
        int.TryParse(argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out number);

    static LiteralExpressionSyntax Literal(object value) => new(value, SourceLocation.Start);
}
