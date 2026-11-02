// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Rebuild.Parity;

/// <summary>
/// An observation that waits to be recorded, with the prologue the Extractor reported it for.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Observation">The observation.</param>
public record QueuedParity(Guid Prologue, ParityObservation Observation);
