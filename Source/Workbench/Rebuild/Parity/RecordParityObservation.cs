// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    /// Handles the command, refusing an observation with no command shape.
    /// </summary>
    /// <returns>The observation recorded, or a refusal.</returns>
    public Result<ParityObserved, ValidationResult> Handle() =>
        Shape.IsNone
            ? ValidationResult.Error("An observation needs the shape of the command it is about")
            : new ParityObserved(Observation, Occurred, Method, Path, Shape, Trace, LegacyStatus, ShadowStatus, Outcome);
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
