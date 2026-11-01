// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Rebuild.Drift;

/// <summary>
/// Acknowledges a piece of drift: events are to be suggested for it. The suggestions themselves are made on the Events step.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Key">The drift.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record AcknowledgeDrift(PrologueId Prologue, DriftKey Key)
{
    /// <summary>
    /// Handles the command, refusing drift that does not exist or has been acknowledged.
    /// </summary>
    /// <param name="drift">The drift, read under the command's consistency boundary.</param>
    /// <returns>The acknowledgement, or a refusal.</returns>
    public Result<DriftAcknowledged, ValidationResult> Handle(ModelDrift? drift) =>
        drift?.Items.FirstOrDefault(item => item.Key == Key) switch
        {
            null => ValidationResult.Error("This drift is not known"),
            { Acknowledged: true } => ValidationResult.Error("Events were suggested for this already"),
            { } item => new DriftAcknowledged(item.Key, item.Shape)
        };
}

/// <summary>
/// A piece of drift was acknowledged: events are to be suggested for it.
/// </summary>
/// <param name="Key">The drift.</param>
/// <param name="Shape">The command shape it is about.</param>
[EventType]
public record DriftAcknowledged(DriftKey Key, CommandShape Shape);
