// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents a provisional slice the <see cref="CaptureAnalyzer"/> derived from a single correlated capture, tagged
/// with the module and feature it belongs to so the <see cref="HeuristicModelBuilder"/> can aggregate drafts that
/// describe the same behavior across many captures.
/// </summary>
/// <param name="Module">The provisional module name the slice belongs to.</param>
/// <param name="Feature">The provisional feature name the slice belongs to.</param>
/// <param name="Name">The provisional slice name.</param>
/// <param name="Type">The inferred slice type.</param>
/// <param name="Commands">The command derived for the slice; empty when none.</param>
/// <param name="Events">The events derived for the slice.</param>
/// <param name="ReadModels">The read model derived for the slice; empty when none.</param>
/// <param name="Projections">The projection derived for the slice; empty when none.</param>
public record SliceDraft(
    string Module,
    string Feature,
    string Name,
    ExtractedSliceType Type,
    IReadOnlyList<ExtractedCommand> Commands,
    IReadOnlyList<ExtractedEvent> Events,
    IReadOnlyList<ExtractedReadModel> ReadModels,
    IReadOnlyList<ExtractedProjection> Projections);
