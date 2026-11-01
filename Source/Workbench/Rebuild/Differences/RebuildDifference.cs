// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Rebuild.Parity;

namespace Cratis.Prologue.Workbench.Rebuild.Differences;

/// <summary>
/// One request the legacy system and the new one decided differently, with the identities in its path so it can be followed.
/// </summary>
/// <param name="Observation">The observation.</param>
/// <param name="Occurred">When it happened.</param>
/// <param name="Shape">The command shape.</param>
/// <param name="Path">The path with the identities in it.</param>
/// <param name="LegacyStatus">The status the legacy system answered with.</param>
/// <param name="LegacyDecision">What the legacy system decided.</param>
/// <param name="ShadowStatus">The status the new system answered with.</param>
/// <param name="ShadowDecision">What the new system decided.</param>
/// <param name="Resolution">What was decided about the difference.</param>
public record RebuildDifference(
    ObservationId Observation,
    DateTimeOffset Occurred,
    CommandShape Shape,
    RequestPath Path,
    HttpStatus LegacyStatus,
    DecisionOutcome LegacyDecision,
    HttpStatus ShadowStatus,
    DecisionOutcome ShadowDecision,
    DifferenceResolution Resolution)
{
    /// <summary>
    /// Describes a mismatch that was observed, as an open difference.
    /// </summary>
    /// <param name="observed">The observation.</param>
    /// <returns>The open difference.</returns>
    public static RebuildDifference From(ParityObserved observed) => new(
        observed.Observation,
        observed.Occurred,
        observed.Shape,
        observed.Path,
        observed.LegacyStatus,
        DecisionRules.Classify(observed.LegacyStatus),
        observed.ShadowStatus,
        DecisionRules.Classify(observed.ShadowStatus),
        DifferenceResolution.Open);
}
