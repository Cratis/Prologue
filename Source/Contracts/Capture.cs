// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents one correlated capture stored in MongoDB — an aggregate of the observations from different sources
/// that the correlator decided belong together, plus the moment the capture is anchored to.
/// </summary>
/// <param name="Id">The unique identifier of the capture.</param>
/// <param name="Occurred">The moment the capture is anchored to (the earliest correlated observation).</param>
/// <param name="Entries">The correlated observations that make up the capture.</param>
/// <param name="PrologueId">The Prologue the capture belongs to; <see cref="Guid.Empty"/> when not associated with one.</param>
public record Capture(Guid Id, DateTimeOffset Occurred, IReadOnlyList<Observation> Entries, Guid PrologueId = default);
