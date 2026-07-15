// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Collects every distinct name that appears in a provisional event model — modules, features, slices, commands,
/// events, read models, projections, and their properties — so the refinement pass can ask the language model to
/// improve them all in one round without ever touching the structure.
/// </summary>
public static class NameCollector
{
    /// <summary>
    /// Collects the distinct names that appear across a provisional event model, in a stable order.
    /// </summary>
    /// <param name="result">The provisional event model.</param>
    /// <returns>The distinct names.</returns>
    public static IReadOnlyList<string> Collect(ExtractionResult result)
    {
        var names = new List<string>();

        foreach (var module in result.Modules)
        {
            names.Add(module.Name);
            foreach (var feature in module.Features)
            {
                CollectFeature(feature, names);
            }
        }

        return [.. names.Distinct()];
    }

    static void CollectFeature(ExtractedFeature feature, List<string> names)
    {
        names.Add(feature.Name);

        foreach (var slice in feature.Slices)
        {
            names.Add(slice.Name);
            names.AddRange(slice.Commands.Select(command => command.Name));
            names.AddRange(slice.Commands.SelectMany(command => command.Properties).Select(property => property.Name));
            names.AddRange(slice.Events.Select(@event => @event.Name));
            names.AddRange(slice.Events.SelectMany(@event => @event.Properties).Select(property => property.Name));
            names.AddRange(slice.ReadModels.Select(readModel => readModel.Name));
            names.AddRange(slice.ReadModels.SelectMany(readModel => readModel.Properties).Select(property => property.Name));
            names.AddRange(slice.Projections.Select(projection => projection.Name));
        }

        foreach (var subFeature in feature.SubFeatures)
        {
            CollectFeature(subFeature, names);
        }
    }
}
