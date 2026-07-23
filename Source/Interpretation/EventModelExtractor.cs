// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents an <see cref="IExtractEventModel"/> that first builds the deterministic structure from the captures
/// and then hands it to the refiner to name into domain language.
/// </summary>
/// <param name="heuristics">The deterministic model builder.</param>
/// <param name="refiner">The language-model naming refinement.</param>
public class EventModelExtractor(IBuildHeuristicModel heuristics, IRefineExtraction refiner) : IExtractEventModel
{
    /// <inheritdoc/>
    public async Task<ExtractionResult> Extract(Guid prologueId, IReadOnlyList<Capture> captures, CancellationToken cancellationToken = default)
    {
        if (captures.Count == 0)
        {
            return ExtractionResult.Empty(prologueId);
        }

        var heuristic = heuristics.Build(prologueId, captures);
        var refined = await refiner.Refine(heuristic, captures, cancellationToken);

        // Order slices producers-first deterministically — State View / Automation / Translation slices consume
        // events emitted by State Change slices, so the State Change slices must come before them.
        return SliceOrdering.Order(refined);
    }
}
