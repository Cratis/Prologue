// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents a slice inferred for a feature — one behavior with its command, events, read model, and projection.
/// The single command/read-model/projection each modeled as a list of zero or one so the structure never carries a
/// nullable property, which keeps it safe to store on a Chronicle event and in a MongoDB read model.
/// </summary>
/// <param name="Name">The name of the slice (the action, for example <c>Register</c>).</param>
/// <param name="Type">The type of the slice.</param>
/// <param name="Commands">The command the slice accepts; empty for slices that do not accept a command.</param>
/// <param name="Events">The events the slice produces or reacts to.</param>
/// <param name="ReadModels">The read model the slice builds; empty for slices that build none.</param>
/// <param name="Projections">The projection the slice defines; empty for slices that define none.</param>
public record ExtractedSlice(
    string Name,
    ExtractedSliceType Type,
    IReadOnlyList<ExtractedCommand> Commands,
    IReadOnlyList<ExtractedEvent> Events,
    IReadOnlyList<ExtractedReadModel> ReadModels,
    IReadOnlyList<ExtractedProjection> Projections);
