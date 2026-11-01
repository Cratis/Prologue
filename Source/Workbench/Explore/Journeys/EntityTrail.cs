// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Journeys;

/// <summary>
/// The commands captured for one thing, in the order they arrived.
/// </summary>
/// <param name="Kind">What the thing is.</param>
/// <param name="Id">Its identity.</param>
/// <param name="Steps">The command shapes captured for it, a repeat of the same step in a row counted once.</param>
public record EntityTrail(EntityKind Kind, EntityId Id, IEnumerable<CommandShape> Steps);
