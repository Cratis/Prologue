// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Differences;

/// <summary>
/// Accepts a difference: the new slice decided differently from the legacy system and that is right. This is recorded as a decision.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Observation">The observation that shows the difference.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record AcceptDifference(PrologueId Prologue, ObservationId Observation)
{
    /// <summary>
    /// Handles the command, refusing a difference that does not exist or is resolved already.
    /// </summary>
    /// <param name="differences">The differences, read under the command's consistency boundary.</param>
    /// <returns>The difference accepted, or a refusal.</returns>
    public Result<DifferenceAccepted, ValidationResult> Handle(RebuildDifferences? differences) =>
        RebuildDifferences.Open(differences, Observation) is { } difference
            ? new DifferenceAccepted(difference.Observation, difference.Shape)
            : ValidationResult.Error("This difference is not open: it does not exist or has been resolved already");
}

/// <summary>
/// A difference between the legacy system and the new slice was accepted: the new behavior is the intended one.
/// </summary>
/// <param name="Observation">The observation that shows the difference.</param>
/// <param name="Shape">The command shape.</param>
[EventType]
public record DifferenceAccepted(ObservationId Observation, CommandShape Shape);
