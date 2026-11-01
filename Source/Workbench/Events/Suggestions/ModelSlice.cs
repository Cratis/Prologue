// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Discover;

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A slice of the interpreted model. Its events are references to suggestions, so the names people chose apply wherever the event appears.
/// </summary>
/// <param name="Module">The module the slice belongs to.</param>
/// <param name="Feature">The feature the slice belongs to.</param>
/// <param name="Name">The slice name.</param>
/// <param name="Type">The kind of slice.</param>
/// <param name="Description">What the slice does, in words; empty when nobody described it.</param>
/// <param name="Commands">The commands of the slice.</param>
/// <param name="Events">The suggested events the slice produces.</param>
/// <param name="ReadModels">The read models of the slice.</param>
/// <param name="Projections">The projections of the slice.</param>
/// <param name="Constraints">The constraints of the slice.</param>
public record ModelSlice(
    ModuleName Module,
    FeatureName Feature,
    SliceName Name,
    ExtractedSliceType Type,
    string Description,
    IEnumerable<ModelCommand> Commands,
    IEnumerable<SuggestionKey> Events,
    IEnumerable<ModelReadModel> ReadModels,
    IEnumerable<ModelProjection> Projections,
    IEnumerable<ModelConstraint> Constraints);
