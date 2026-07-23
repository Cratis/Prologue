// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Extracts an event model from a Prologue's captures by combining the deterministic heuristic structure with the
/// language-model naming refinement.
/// </summary>
public interface IExtractEventModel
{
    /// <summary>
    /// Extracts the event model for a Prologue from its captures.
    /// </summary>
    /// <param name="prologueId">The Prologue the captures belong to.</param>
    /// <param name="captures">The correlated captures to analyze.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The extracted <see cref="ExtractionResult"/>.</returns>
    Task<ExtractionResult> Extract(Guid prologueId, IReadOnlyList<Capture> captures, CancellationToken cancellationToken = default);
}
