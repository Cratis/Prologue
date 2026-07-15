// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents a single thing observed by a source — the common unit that flows through the capture pipeline
/// before correlation. Every source emits observations regardless of its kind.
/// </summary>
/// <param name="Source">The kind of source the observation came from.</param>
/// <param name="Occurred">The moment the observed thing happened.</param>
/// <param name="Payload">The source-specific metadata.</param>
public record Observation(SourceKind Source, DateTimeOffset Occurred, ObservationPayload Payload);
