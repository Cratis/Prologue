// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Combines the models of separate parts of the captures into one.
/// </summary>
public static class ModelMerge
{
    /// <summary>
    /// Adds a suffix to the names of every slice of a model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="suffix">The suffix; empty leaves the model as it is.</param>
    /// <returns>The model with the slices renamed.</returns>
    public static ExtractionResult Suffixed(ExtractionResult model, string suffix) =>
        suffix.Length == 0
            ? model
            : model with { Modules = [.. model.Modules.Select(module => module with { Features = [.. module.Features.Select(feature => Suffixed(feature, suffix))] })] };

    /// <summary>
    /// Combines models into one: modules and features of the same name are joined and their slices follow each other in the order of the models.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="models">The models.</param>
    /// <returns>The combined model.</returns>
    public static ExtractionResult Combine(Guid prologue, IEnumerable<ExtractionResult> models) =>
        new(
            prologue,
            [
                .. models.SelectMany(model => model.Modules).GroupBy(module => module.Name).Select(modules => new ExtractedModule(
                    modules.Key,
                    CombineFeatures(modules.SelectMany(module => module.Features)),
                    modules.Select(module => module.Description).FirstOrDefault(description => description.Length > 0) ?? string.Empty))
            ]);

    static ExtractedFeature Suffixed(ExtractedFeature feature, string suffix) =>
        feature with
        {
            Slices = [.. feature.Slices.Select(slice => slice.Type == ExtractedSliceType.StateView ? slice : slice with { Name = $"{slice.Name}{suffix}" })],
            SubFeatures = [.. feature.SubFeatures.Select(sub => Suffixed(sub, suffix))]
        };

    static IReadOnlyList<ExtractedFeature> CombineFeatures(IEnumerable<ExtractedFeature> features) =>
    [
        .. features.GroupBy(feature => feature.Name).Select(group => new ExtractedFeature(
            group.Key,
            [.. group.SelectMany(feature => feature.SubFeatures)],
            MergeViews(group.SelectMany(feature => feature.Slices)),
            group.Select(feature => feature.Description).FirstOrDefault(description => description.Length > 0) ?? string.Empty))
    ];

    // A read model seen in several parts of the captures is one slice: the parts only differ in the events that build it.
    static List<ExtractedSlice> MergeViews(IEnumerable<ExtractedSlice> slices)
    {
        var merged = new List<ExtractedSlice>();
        foreach (var slice in slices)
        {
            var index = slice.Type == ExtractedSliceType.StateView ? merged.FindIndex(existing => existing.Type == ExtractedSliceType.StateView && existing.Name == slice.Name) : -1;
            if (index < 0)
            {
                merged.Add(slice);
                continue;
            }

            merged[index] = merged[index] with
            {
                Projections =
                [
                    .. merged[index].Projections.Concat(slice.Projections).GroupBy(projection => projection.Name)
                        .Select(group => new ExtractedProjection(group.Key, [.. group.SelectMany(projection => projection.SourceEvents).Distinct(StringComparer.OrdinalIgnoreCase)]))
                ]
            };
        }

        return merged;
    }
}
