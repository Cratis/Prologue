// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents a single line written to a rolling JSON-lines capture file — one observation tagged with the id of
/// the correlated capture it belongs to, so captures can be reconstructed across per-source-kind files.
/// </summary>
/// <param name="CaptureId">The id of the correlated capture the observation belongs to.</param>
/// <param name="Occurred">When the observation happened.</param>
/// <param name="Source">The kind of source the observation came from.</param>
/// <param name="Payload">The source-specific metadata.</param>
public record CapturedEntry(Guid CaptureId, DateTimeOffset Occurred, SourceKind Source, ObservationPayload Payload);
