// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// Records that the Extractor mirrored a command to the new system and compared its answer with the legacy system's.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Observation">The identity of the observation.</param>
/// <param name="Occurred">When the legacy system answered.</param>
/// <param name="Method">The HTTP method.</param>
/// <param name="Path">The request path, without its query string.</param>
/// <param name="Shape">The shape of the command.</param>
/// <param name="Trace">The trace the request carried, or none.</param>
/// <param name="LegacyStatus">The status the legacy system answered with.</param>
/// <param name="ShadowStatus">The status the new system answered with, or none when it did not answer.</param>
/// <param name="Outcome">How the two compare.</param>
[Command]
public record RecordParityObservation(
    PrologueId Prologue,
    ObservationId Observation,
    DateTimeOffset Occurred,
    HttpMethodName Method,
    RequestPath Path,
    CommandShape Shape,
    TraceId Trace,
    HttpStatus LegacyStatus,
    HttpStatus ShadowStatus,
    ParityOutcome Outcome)
{
    /// <summary>
    /// Handles the command, refusing an observation with no command shape. An observation already recorded is a retry of the
    /// Extractor and records nothing, so the board never counts it twice.
    /// </summary>
    /// <param name="recent">The latest observations of the prologue, which hold the identity of any retry.</param>
    /// <returns>The observation recorded, nothing for a retry, or a refusal.</returns>
    public Result<IEnumerable<ParityObserved>, ValidationResult> Handle([Unprotected("A retry racing the projection may record once more; the worker records one observation at a time, so the window is the projection lag")] RecentParity? recent)
    {
        if (Shape.IsNone)
        {
            return ValidationResult.Error("An observation needs the shape of the command it is about");
        }

        IEnumerable<ParityObserved> events = recent?.Observations.Any(observed => observed.Observation == Observation) == true
            ? []
            : [new ParityObserved(Observation, Occurred, Method, Path, Shape, Trace, LegacyStatus, ShadowStatus, Outcome)];
        return Result<IEnumerable<ParityObserved>, ValidationResult>.Success(events);
    }
}

/// <summary>
/// A command was mirrored to the new system and its answer compared with the legacy system's.
/// </summary>
/// <param name="Observation">The identity of the observation.</param>
/// <param name="Occurred">When the legacy system answered.</param>
/// <param name="Method">The HTTP method.</param>
/// <param name="Path">The request path, without its query string.</param>
/// <param name="Shape">The shape of the command.</param>
/// <param name="Trace">The trace the request carried, or none.</param>
/// <param name="LegacyStatus">The status the legacy system answered with.</param>
/// <param name="ShadowStatus">The status the new system answered with, or none when it did not answer.</param>
/// <param name="Outcome">How the two compare.</param>
[EventType]
public record ParityObserved(
    ObservationId Observation,
    DateTimeOffset Occurred,
    HttpMethodName Method,
    RequestPath Path,
    CommandShape Shape,
    TraceId Trace,
    HttpStatus LegacyStatus,
    HttpStatus ShadowStatus,
    ParityOutcome Outcome);
