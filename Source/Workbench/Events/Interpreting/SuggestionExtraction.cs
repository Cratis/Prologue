// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Discover;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Turns what an interpretation found into suggested events and the structure around them. The heuristic model gives the stable part (the
/// mechanical names that identify a suggestion), the renames give what the language model called things, and the captures give the evidence.
/// </summary>
public static class SuggestionExtraction
{
    /// <summary>
    /// Extracts the suggestions and the structure.
    /// </summary>
    /// <param name="partitions">The parts of the captures that were interpreted.</param>
    /// <param name="heuristic">The model the heuristics built.</param>
    /// <param name="final">The model the interpretation ended with, holding the descriptions and the system name; null when it ended without one.</param>
    /// <param name="renames">What the language model renamed, from the mechanical name; empty when it renamed nothing.</param>
    /// <param name="facts">What else is known about the prologue.</param>
    /// <returns>The suggestions in the order of the model, and the structure.</returns>
    public static (IReadOnlyList<SuggestedEvent> Suggestions, InterpretedModel Model) From(
        IReadOnlyList<CapturePartition> partitions,
        ExtractionResult heuristic,
        ExtractionResult? final,
        IReadOnlyDictionary<string, string> renames,
        EvidenceFacts facts)
    {
        var attributions = CaptureAttribution.Attribute(partitions);
        string Rename(string name) => renames.TryGetValue(name, out var refined) && !string.IsNullOrWhiteSpace(refined) ? refined : name;

        var suggestions = new Dictionary<SuggestionKey, SuggestedEvent>();
        var slices = new List<ModelSlice>();
        foreach (var module in heuristic.Modules)
        {
            var shown = final?.Modules.FirstOrDefault(candidate => candidate.Name == Rename(module.Name));
            foreach (var (feature, slice) in module.Features.SelectMany(feature => feature.Slices.Select(slice => (feature, slice))))
            {
                foreach (var @event in slice.Events)
                {
                    var key = CaptureAttribution.KeyOf(module.Name, @event.Name);
                    suggestions[key] = suggestions.TryGetValue(key, out var existing)
                        ? existing with { Properties = Union(existing.Properties, @event.Properties, Rename) }
                        : Suggest(key, module.Name, @event, attributions.GetValueOrDefault(key) ?? new Attribution(), facts, Rename);
                }

                slices.Add(Slice(module, feature, slice, shown, Rename));
            }
        }

        return ([.. suggestions.Values], new InterpretedModel(final?.SystemName ?? string.Empty, slices));
    }

    static SuggestedEvent Suggest(SuggestionKey key, string module, ExtractedEvent @event, Attribution attribution, EvidenceFacts facts, Func<string, string> rename)
    {
        var shapes = attribution.Shapes.Select(shape => new ShapeEvidence(shape.Key, shape.Value)).OrderByDescending(shape => shape.Captures).ThenBy(shape => shape.Shape.Value, StringComparer.Ordinal).ToList();
        var code = facts.CodePoints
            .Where(point => shapes.Exists(shape => SuggestionNotes.Same(shape.Shape, point.Shape)))
            .Select(point => new CodeEvidence(point.File, point.Line, point.Rule.Value))
            .Distinct()
            .ToList();
        var scenarios = facts.Shapes
            .Where(profile => shapes.Exists(shape => shape.Shape == profile.Shape))
            .SelectMany(profile => profile.Scenarios)
            .Select(scenario => scenario.Scenario.Value)
            .Where(scenario => scenario.Length > 0)
            .Distinct()
            .Order()
            .ToList();
        var tables = attribution.Tables
            .OrderBy(table => table.Key.Table, StringComparer.Ordinal).ThenBy(table => table.Key.Operation, StringComparer.Ordinal)
            .Select(table => new TableEvidence(table.Key.Schema, table.Key.Table, table.Key.Operation, [.. table.Value.Order()]))
            .ToList();
        var commandCaptures = shapes.Sum(shape => facts.Shapes.FirstOrDefault(profile => profile.Shape == shape.Shape)?.Captures ?? shape.Captures);

        return new SuggestedEvent(
            key,
            @event.Name,
            rename(@event.Name),
            rename(module),
            TableEvents.StreamOf(@event.Name),
            [.. @event.Properties.Select(property => Property(property, rename))],
            SuggestionConfidence.For(shapes.Sum(shape => shape.Captures), commandCaptures, code.Count > 0),
            new SuggestionEvidence(shapes, tables, code, scenarios),
            SuggestionNotes.For(attribution, facts));
    }

    static ModelSlice Slice(ExtractedModule module, ExtractedFeature feature, ExtractedSlice slice, ExtractedModule? shown, Func<string, string> rename)
    {
        var shownSlice = shown?.Features.FirstOrDefault(candidate => candidate.Name == rename(feature.Name))?.Slices.FirstOrDefault(candidate => candidate.Name == rename(slice.Name));
        SuggestionKey KeyOf(string eventName) => CaptureAttribution.KeyOf(module.Name, eventName);
        var known = slice.Events.Select(@event => @event.Name).ToHashSet();

        return new ModelSlice(
            rename(module.Name),
            rename(feature.Name),
            rename(slice.Name),
            slice.Type,
            shownSlice?.Description ?? string.Empty,
            [
                .. slice.Commands.Select(command => new ModelCommand(
                    rename(command.Name),
                    [.. command.Properties.Select(property => Property(property, rename))],
                    [.. command.Validations.Select(rule => new ModelRule(rename(rule.Property), rule.Kind, rule.Argument, rule.Message))],
                    shownSlice?.Commands.FirstOrDefault(candidate => candidate.Name == rename(command.Name))?.Description ?? string.Empty))
            ],
            [.. slice.Events.Select(@event => KeyOf(@event.Name))],
            [.. slice.ReadModels.Select(readModel => new ModelReadModel(rename(readModel.Name), [.. readModel.Properties.Select(property => Property(property, rename))]))],
            [.. slice.Projections.Select(projection => new ModelProjection(rename(projection.Name), [.. projection.SourceEvents.Select(KeyOf)]))],
            [.. slice.Constraints.Select(constraint => new ModelConstraint(constraint.Name, rename(constraint.Property), KeyOf(constraint.OnEvent)))]);
    }

    static SuggestedProperty Property(ExtractedProperty property, Func<string, string> rename) =>
        new(rename(property.Name), property.Type, property.IsRequired, property.MaxLength);

    static List<SuggestedProperty> Union(IEnumerable<SuggestedProperty> existing, IReadOnlyList<ExtractedProperty> more, Func<string, string> rename)
    {
        var kept = existing.ToList();
        kept.AddRange(more.Select(property => Property(property, rename)).Where(property => kept.TrueForAll(candidate => candidate.Name != property.Name)));
        return kept;
    }
}
