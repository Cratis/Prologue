// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// A slice of the accepted model.
/// </summary>
/// <param name="Name">The slice name.</param>
/// <param name="Type">The kind of slice.</param>
/// <param name="Description">What the slice does, in words; empty when nobody described it.</param>
/// <param name="Shapes">The command shapes the slice's events were captured from. A Rebuild slice that replaces the slice is added for exactly these (<c language="csharp">AddRebuildSlice</c>); empty for slices no capture evidences.</param>
/// <param name="Commands">The commands.</param>
/// <param name="Events">The events, without the rejected ones.</param>
/// <param name="ReadModels">The read models.</param>
/// <param name="Projections">The projections.</param>
/// <param name="Constraints">The constraints.</param>
/// <param name="Rules">The rules the evidence suggests and the choices made for consistency boundaries that concern the slice.</param>
/// <param name="IsOpen">Whether any event of the slice is still open.</param>
public record AcceptedSlice(
    SliceName Name,
    ExtractedSliceType Type,
    string Description,
    IEnumerable<CommandShape> Shapes,
    IEnumerable<ModelCommand> Commands,
    IEnumerable<AcceptedEvent> Events,
    IEnumerable<ModelReadModel> ReadModels,
    IEnumerable<AcceptedProjection> Projections,
    IEnumerable<AcceptedConstraint> Constraints,
    IEnumerable<AcceptedRule> Rules,
    bool IsOpen);
