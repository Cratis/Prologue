// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Orders the slices within an extracted event model so that producers come before consumers: State View,
/// Automation, and Translation slices consume events emitted by State Change slices, so every State Change slice
/// is placed before them. The order is deterministic and applied after the model is built, rather than relying on
/// the language model to get it right.
/// </summary>
public static class SliceOrdering
{
    /// <summary>
    /// Orders the slices within every feature of the extracted model so State Change slices precede the slices
    /// that consume their events.
    /// </summary>
    /// <param name="result">The extracted event model to order.</param>
    /// <returns>An equivalent <see cref="ExtractionResult"/> with each feature's slices ordered producers-first.</returns>
    public static ExtractionResult Order(ExtractionResult result) =>
        result with { Modules = [.. result.Modules.Select(OrderModule)] };

    static ExtractedModule OrderModule(ExtractedModule module) =>
        module with { Features = [.. module.Features.Select(OrderFeature)] };

    static ExtractedFeature OrderFeature(ExtractedFeature feature) =>
        feature with
        {
            SubFeatures = [.. feature.SubFeatures.Select(OrderFeature)],
            Slices = [.. feature.Slices.OrderBy(slice => slice.Type == ExtractedSliceType.StateChange ? 0 : 1)]
        };
}
