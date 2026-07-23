// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Screenplay.Diagnostics;
using Cratis.Screenplay.Printing;
using Cratis.Screenplay.Syntax;
using Cratis.Screenplay.Syntax.Projections;

namespace Cratis.Prologue.Screenplay;

/// <summary>
/// Represents an implementation of <see cref="IScreenplayGenerator"/> that maps the extracted module → feature →
/// slice tree onto the Screenplay syntax tree and prints it. Modules, features, and slices carry their descriptions
/// through; commands declare their properties, validation rules, and <c>produces</c> mappings; slice events become
/// event declarations; read models become an all-instances query and the projection that builds them; and observed
/// uniqueness becomes <c>constraint</c> declarations. Every node uses <see cref="SourceLocation.Start"/> as a
/// placeholder — the printer never reads locations.
/// </summary>
/// <param name="printer">The <see cref="IScreenplayPrinter"/> that renders the syntax tree to <c>.play</c> source text.</param>
public class ScreenplayGenerator(IScreenplayPrinter printer) : IScreenplayGenerator
{
    /// <inheritdoc/>
    public ApplicationSyntax Build(ExtractionResult result)
    {
        var events = EventsOf(result);
        return new([], [], [], [.. result.Modules.Select(module => Module(module, events))], SourceLocation.Start);
    }

    /// <inheritdoc/>
    public string Generate(ExtractionResult result) => printer.Print(Build(result));

    static ModuleSyntax Module(ExtractedModule module, IReadOnlyDictionary<string, ExtractedEvent> events) =>
        new(
            module.Name,
            [],
            [.. module.Features.Select(feature => Feature(feature, events))],
            SourceLocation.Start,
            DescriptionOf(module.Description));

    static FeatureSyntax Feature(ExtractedFeature feature, IReadOnlyDictionary<string, ExtractedEvent> events) =>
        new(
            feature.Name,
            [.. feature.SubFeatures.Select(subFeature => Feature(subFeature, events))],
            [.. feature.Slices.Select(slice => Slice(slice, events))],
            SourceLocation.Start,
            DescriptionOf(feature.Description));

    static SliceSyntax Slice(ExtractedSlice slice, IReadOnlyDictionary<string, ExtractedEvent> events) =>
        new(
            TypeOf(slice.Type),
            slice.Name,
            [.. slice.Events.Select(Event)],
            [.. slice.Commands.Select(command => Command(command, slice.Events))],
            [.. slice.ReadModels.Select(Query)],
            slice.Projections.Count > 0 ? Projection(slice.Projections[0], slice.ReadModels.Count > 0 ? slice.ReadModels[0] : null, events) : null,
            [],
            [],
            [],
            [.. slice.Constraints.Select(Constraint)],
            [],
            SourceLocation.Start,
            DescriptionOf(slice.Description));

    static CommandSyntax Command(ExtractedCommand command, IReadOnlyList<ExtractedEvent> produced) =>
        new(
            command.Name,
            [.. command.Properties.Select(Property)],
            null,
            Validations(command.Validations),
            [.. produced.Select(@event => Produces(@event, command))],
            null,
            SourceLocation.Start);

    static IEnumerable<ValidateSyntax> Validations(IReadOnlyList<ExtractedValidationRule> validations)
    {
        var rules = validations.Select(Rule).OfType<ValidationRuleSyntax>().ToList();
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

    static ProducesSyntax Produces(ExtractedEvent @event, ExtractedCommand command)
    {
        var mappings = @event.Properties
            .Where(property => command.Properties.Any(candidate => candidate.Name == property.Name))
            .Select(property => new PropertyMappingSyntax(
                Camelize(property.Name),
                new PathExpressionSyntax(Camelize(property.Name), SourceLocation.Start),
                SourceLocation.Start));
        return new(@event.Name, null, [.. mappings], SourceLocation.Start);
    }

    static EventSyntax Event(ExtractedEvent @event) =>
        new(@event.Name, [.. @event.Properties.Select(Property)], SourceLocation.Start);

    static PropertySyntax Property(ExtractedProperty property) =>
        new(
            Camelize(property.Name),
            new(ScreenplayTypeMapping.TypeFor(property.Type), IsCollection: false, IsOptional: false, SourceLocation.Start),
            SourceLocation.Start);

    static QuerySyntax Query(ExtractedReadModel readModel) =>
        new(
            $"All{readModel.Name}s",
            new(readModel.Name, IsCollection: true, IsOptional: false, SourceLocation.Start),
            null,
            [],
            null,
            SourceLocation.Start);

    static ProjectionSyntax Projection(
        ExtractedProjection projection,
        ExtractedReadModel? readModel,
        IReadOnlyDictionary<string, ExtractedEvent> events) =>
        new(
            projection.Name,
            readModel?.Name,
            null,
            AutoMapMode.Inherit,
            null,
            [.. projection.SourceEvents.Select(sourceEvent => From(sourceEvent, readModel, events))],
            SourceLocation.Start);

    static FromSyntax From(
        string sourceEvent,
        ExtractedReadModel? readModel,
        IReadOnlyDictionary<string, ExtractedEvent> events)
    {
        var mappings = readModel is not null && events.TryGetValue(sourceEvent, out var @event)
            ? readModel.Properties
                .Where(property => @event.Properties.Any(candidate => candidate.Name == property.Name))
                .Select(MappingSyntax (property) => new SetMappingSyntax(
                    Camelize(property.Name),
                    new PathExpressionSyntax(Camelize(property.Name), SourceLocation.Start),
                    SourceLocation.Start))
            : [];
        return new([new EventSpecSyntax(sourceEvent, null, SourceLocation.Start)], null, null, [.. mappings], SourceLocation.Start);
    }

    static ConstraintSyntax Constraint(ExtractedConstraint constraint) =>
        new UniquePropertyConstraintSyntax(constraint.Name, constraint.Property, constraint.OnEvent, SourceLocation.Start);

    static SliceType TypeOf(ExtractedSliceType type) =>
        type switch
        {
            ExtractedSliceType.StateChange => SliceType.StateChange,
            ExtractedSliceType.StateView => SliceType.StateView,
            ExtractedSliceType.Automation => SliceType.Automation,
            _ => SliceType.Translate
        };

    static Dictionary<string, ExtractedEvent> EventsOf(ExtractionResult result)
    {
        var events = new Dictionary<string, ExtractedEvent>(StringComparer.Ordinal);
        foreach (var @event in result.Modules
            .SelectMany(module => module.Features)
            .SelectMany(AllFeatures)
            .SelectMany(feature => feature.Slices)
            .SelectMany(slice => slice.Events))
        {
            events.TryAdd(@event.Name, @event);
        }

        return events;
    }

    static IEnumerable<ExtractedFeature> AllFeatures(ExtractedFeature feature)
    {
        yield return feature;
        foreach (var nested in feature.SubFeatures.SelectMany(AllFeatures))
        {
            yield return nested;
        }
    }

    static string? DescriptionOf(string description) => description.Length == 0 ? null : description;

    static string Camelize(string name) =>
        name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name[1..];

    static bool TryNumber(string argument, out int number) =>
        int.TryParse(argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out number);

    static LiteralExpressionSyntax Literal(object value) => new(value, SourceLocation.Start);
}
