// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Derives a deterministic system name from an extracted event model — used when language-model refinement is
/// disabled or falls back, so the model always carries a system name. The dominant module (the one with the most
/// slices, ties broken alphabetically) names the system; a model without modules falls back to
/// <see cref="Fallback"/>.
/// </summary>
public static class SystemNameDeriver
{
    /// <summary>
    /// The system name used when the model has no modules to derive one from.
    /// </summary>
    public const string Fallback = "CapturedSystem";

    /// <summary>
    /// Derives the system name for an extracted event model.
    /// </summary>
    /// <param name="model">The extracted event model to derive the name from.</param>
    /// <returns>The derived system name.</returns>
    public static string Derive(ExtractionResult model)
    {
        var dominant = model.Modules
            .Where(module => module.Name.Length > 0)
            .OrderByDescending(SliceCount)
            .ThenBy(module => module.Name, StringComparer.Ordinal)
            .FirstOrDefault();

        return dominant?.Name ?? Fallback;
    }

    static int SliceCount(ExtractedModule module) => module.Features.Sum(SliceCount);

    static int SliceCount(ExtractedFeature feature) => feature.Slices.Count + feature.SubFeatures.Sum(SliceCount);
}
