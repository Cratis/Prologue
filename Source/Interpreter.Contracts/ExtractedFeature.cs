// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents a feature inferred for a module — a grouping of slices, optionally nesting sub-features.
/// </summary>
/// <param name="Name">The name of the feature.</param>
/// <param name="SubFeatures">The sub-features nested within the feature.</param>
/// <param name="Slices">The slices within the feature.</param>
public record ExtractedFeature(
    string Name,
    IReadOnlyList<ExtractedFeature> SubFeatures,
    IReadOnlyList<ExtractedSlice> Slices);
