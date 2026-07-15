// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Defines a strategy that groups observations from different sources into correlated <see cref="Capture"/>s.
/// Implementations decide how observations relate; the time-window heuristic is the default, but the seam allows
/// smarter strategies to replace it.
/// </summary>
public interface ICorrelator
{
    /// <summary>
    /// Adds an observation to the set of observations being considered for correlation.
    /// </summary>
    /// <param name="observation">The <see cref="Observation"/> to add.</param>
    void Add(Observation observation);

    /// <summary>
    /// Produces captures for all observations that have settled — that is, those old enough that no further
    /// correlated observation can arrive for them.
    /// </summary>
    /// <param name="upTo">The current point in time; observations are settled relative to it.</param>
    /// <returns>The settled <see cref="Capture"/>s, removed from the correlator's pending set.</returns>
    IReadOnlyList<Capture> Drain(DateTimeOffset upTo);
}
