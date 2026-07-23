// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Applies a name-refinement map to a provisional event model, rewriting every module, feature, slice, command,
/// event, read-model, projection, and property name while preserving the structure exactly. Names absent from the
/// map keep their provisional value, so a partial map is always safe.
/// </summary>
public static class ModelRenamer
{
    /// <summary>
    /// Rewrites the names in a provisional event model using the given refinement map. Projection source-event
    /// references are rewritten too, so a projection keeps pointing at its (renamed) events.
    /// </summary>
    /// <param name="result">The provisional event model to rewrite.</param>
    /// <param name="renames">The map from provisional name to refined name.</param>
    /// <returns>The rewritten <see cref="ExtractionResult"/>.</returns>
    public static ExtractionResult Apply(ExtractionResult result, IReadOnlyDictionary<string, string> renames)
    {
        string Rename(string name) => renames.TryGetValue(name, out var refined) && !string.IsNullOrWhiteSpace(refined) ? refined : name;

        ExtractedProperty RenameProperty(ExtractedProperty property) => property with { Name = Rename(property.Name) };

        ExtractedSlice RenameSlice(ExtractedSlice slice) => slice with
        {
            Name = Rename(slice.Name),
            Commands = [.. slice.Commands.Select(command => command with { Name = Rename(command.Name), Properties = [.. command.Properties.Select(RenameProperty)] })],
            Events = [.. slice.Events.Select(@event => @event with { Name = Rename(@event.Name), Properties = [.. @event.Properties.Select(RenameProperty)] })],
            ReadModels = [.. slice.ReadModels.Select(readModel => readModel with { Name = Rename(readModel.Name), Properties = [.. readModel.Properties.Select(RenameProperty)] })],
            Projections = [.. slice.Projections.Select(projection => projection with { Name = Rename(projection.Name), SourceEvents = [.. projection.SourceEvents.Select(Rename)] })]
        };

        ExtractedFeature RenameFeature(ExtractedFeature feature) => feature with
        {
            Name = Rename(feature.Name),
            SubFeatures = [.. feature.SubFeatures.Select(RenameFeature)],
            Slices = [.. feature.Slices.Select(RenameSlice)]
        };

        return result with
        {
            Modules = [.. result.Modules.Select(module => module with { Name = Rename(module.Name), Features = [.. module.Features.Select(RenameFeature)] })]
        };
    }
}
