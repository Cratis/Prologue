// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Builds a deterministic, provisional event model from a Prologue's captures — the structural skeleton the LLM
/// refinement pass then names into domain language.
/// </summary>
public interface IBuildHeuristicModel
{
    /// <summary>
    /// Builds the provisional event model for a Prologue from its correlated captures.
    /// </summary>
    /// <param name="prologueId">The Prologue the captures belong to.</param>
    /// <param name="captures">The correlated captures to analyze.</param>
    /// <returns>The provisional <see cref="ExtractionResult"/>.</returns>
    ExtractionResult Build(Guid prologueId, IReadOnlyList<Capture> captures);
}
