// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Applies a language-model refinement to a provisional event model, rewriting every module, feature, slice,
/// command, event, read-model, projection, and property name while preserving the structure exactly, and then
/// applying the descriptions and system name the model contributed. Names absent from the rename map keep their
/// provisional value, so a partial map is always safe.
/// </summary>
public static class ModelRenamer
{
    /// <summary>
    /// Applies a full <see cref="ModelRefinement"/> to a provisional event model — the renames first, then the
    /// descriptions (whose keys refer to the renamed names), then the system name. An empty system name in the
    /// refinement leaves the model's existing system name untouched.
    /// </summary>
    /// <param name="result">The provisional event model to refine.</param>
    /// <param name="refinement">The refinement the language model returned.</param>
    /// <returns>The refined <see cref="ExtractionResult"/>.</returns>
    public static ExtractionResult Apply(ExtractionResult result, ModelRefinement refinement)
    {
        var refined = ApplyDescriptions(Apply(result, refinement.Renames), refinement.Descriptions);
        return refinement.SystemName is { Length: > 0 } systemName ? refined with { SystemName = systemName } : refined;
    }

    /// <summary>
    /// Rewrites the names in a provisional event model using the given refinement map. Projection source-event and
    /// constraint event references are rewritten too, so they keep pointing at their (renamed) events.
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
            Projections = [.. slice.Projections.Select(projection => projection with { Name = Rename(projection.Name), SourceEvents = [.. projection.SourceEvents.Select(Rename)] })],
            Constraints = [.. slice.Constraints.Select(constraint => constraint with { OnEvent = Rename(constraint.OnEvent) })]
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

    static ExtractionResult ApplyDescriptions(ExtractionResult result, IReadOnlyDictionary<string, string> descriptions)
    {
        if (descriptions.Count == 0)
        {
            return result;
        }

        string Describe(string key, string current) =>
            descriptions.TryGetValue(key, out var description) && !string.IsNullOrWhiteSpace(description) ? description : current;

        ExtractedSlice DescribeSlice(ExtractedSlice slice, string featurePath)
        {
            var slicePath = $"{featurePath}/{slice.Name}";
            return slice with
            {
                Description = Describe($"slice:{slicePath}", slice.Description),
                Commands = [.. slice.Commands.Select(command => command with { Description = Describe($"command:{slicePath}/{command.Name}", command.Description) })]
            };
        }

        ExtractedFeature DescribeFeature(ExtractedFeature feature, string path)
        {
            var featurePath = $"{path}/{feature.Name}";
            return feature with
            {
                Description = Describe($"feature:{featurePath}", feature.Description),
                SubFeatures = [.. feature.SubFeatures.Select(subFeature => DescribeFeature(subFeature, featurePath))],
                Slices = [.. feature.Slices.Select(slice => DescribeSlice(slice, featurePath))]
            };
        }

        return result with
        {
            Modules =
            [
                .. result.Modules.Select(module => module with
                {
                    Description = Describe($"module:{module.Name}", module.Description),
                    Features = [.. module.Features.Select(feature => DescribeFeature(feature, module.Name))]
                })
            ]
        };
    }
}
