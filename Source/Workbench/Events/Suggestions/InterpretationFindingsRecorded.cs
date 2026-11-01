// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.NotEvents;

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// An interpretation derived different consistency boundaries or things that are not events than the ones before. The findings replace the ones before.
/// </summary>
/// <param name="Boundaries">Every consistency boundary the evidence raises.</param>
/// <param name="NotEvents">Everything the evidence shows is not an event.</param>
[EventType]
public record InterpretationFindingsRecorded(IEnumerable<ConsistencyBoundary> Boundaries, IEnumerable<NotEvent> NotEvents);
