// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// Turns the accepted model into the <see cref="ExtractionResult"/> the <c language="csharp">ScreenplayGenerator</c> takes. Pure.
/// </summary>
public static class AcceptedModelMapper
{
    /// <summary>
    /// Maps the accepted model.
    /// </summary>
    /// <remarks>
    /// Event names are the final names. The rules and consistency-boundary choices of a slice have no place of their own in an
    /// <see cref="ExtractionResult"/>, so they are appended to the slice's description as <c language="csharp">Rule:</c> lines; unique constraints map to
    /// <see cref="ExtractedConstraint"/>. Module and feature descriptions are not kept.
    /// </remarks>
    /// <param name="model">The accepted model.</param>
    /// <param name="includeOpen">Whether events nobody has decided on are part of the result, as the drafts they are. When false, a slice left without events is left out as well.</param>
    /// <returns>The extraction result for the prologue.</returns>
    public static ExtractionResult ToExtractionResult(AcceptedModel model, bool includeOpen = true)
    {
        var kept = model.Modules.SelectMany(module => module.Features).SelectMany(feature => feature.Slices).SelectMany(slice => slice.Events).Where(@event => includeOpen || !@event.IsOpen).Select(@event => @event.Name.Value).ToHashSet();
        return new(
            model.Id.Value,
            [
                .. model.Modules.Select(module => new ExtractedModule(
                    module.Name.Value,
                    [.. module.Features.Select(feature => new ExtractedFeature(feature.Name.Value, [], [.. feature.Slices.Select(slice => Slice(slice, kept, includeOpen)).OfType<ExtractedSlice>()]))]))
                .Where(module => module.Features.Any(feature => feature.Slices.Count > 0))
            ],
            model.SystemName);
    }

    static ExtractedSlice? Slice(AcceptedSlice slice, HashSet<string> kept, bool includeOpen)
    {
        var events = slice.Events.Where(@event => includeOpen || !@event.IsOpen).ToList();
        if (slice.Events.Any() && events.Count == 0)
        {
            return null;
        }

        var description = string.Join(Environment.NewLine, new[] { slice.Description }.Concat(slice.Rules.Select(rule => $"Rule: {rule.Text}")).Where(line => line.Length > 0));
        return new ExtractedSlice(
            slice.Name.Value,
            slice.Type,
            [.. slice.Commands.Select(command => new ExtractedCommand(command.Name.Value, [.. command.Properties.Select(Property)], [.. command.Rules.Select(rule => new ExtractedValidationRule(rule.Property, rule.Kind, rule.Argument, rule.Message))], command.Description))],
            [.. events.Select(@event => new ExtractedEvent(@event.Name.Value, [.. @event.Properties.Select(Property)]))],
            [.. slice.ReadModels.Select(readModel => new ExtractedReadModel(readModel.Name, [.. readModel.Properties.Select(Property)]))],
            [.. slice.Projections.Select(projection => new ExtractedProjection(projection.Name, [.. projection.Events.Select(@event => @event.Value).Where(kept.Contains)]))],
            [.. slice.Constraints.Where(constraint => kept.Contains(constraint.Event.Value)).Select(constraint => new ExtractedConstraint(constraint.Name, constraint.Property, constraint.Event.Value))],
            description);
    }

    static ExtractedProperty Property(Suggestions.SuggestedProperty property) => new(property.Name, property.Type, property.IsRequired, property.MaxLength);
}
