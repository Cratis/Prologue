// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.NotEvents;
using Cratis.Prologue.Workbench.Events.Suggestions;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// What an interpretation found, ready to be compared with the last one and recorded.
/// </summary>
/// <param name="Suggestions">The events suggested.</param>
/// <param name="Model">The structure around them.</param>
/// <param name="Boundaries">The consistency boundaries the evidence raises.</param>
/// <param name="NotEvents">What the evidence shows is not an event.</param>
public record InterpretationOutcome(
    IReadOnlyList<SuggestedEvent> Suggestions,
    InterpretedModel Model,
    IReadOnlyList<ConsistencyBoundary> Boundaries,
    IReadOnlyList<NotEvent> NotEvents);
