// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Refines the language parts of a heuristically-built event model — the module, feature, slice, command, event,
/// read-model, and property names — into idiomatic domain language. The deterministic heuristics decide the
/// structure; the refiner (typically LLM-backed) decides what everything is called.
/// </summary>
public interface IRefineExtraction
{
    /// <summary>
    /// Refines the names in a provisional event model using the captures as evidence.
    /// </summary>
    /// <param name="model">The provisional event model to refine.</param>
    /// <param name="captures">The captures the model was derived from, available as naming evidence.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The refined <see cref="ExtractionResult"/>.</returns>
    Task<ExtractionResult> Refine(ExtractionResult model, IReadOnlyList<Capture> captures, CancellationToken cancellationToken = default);
}
