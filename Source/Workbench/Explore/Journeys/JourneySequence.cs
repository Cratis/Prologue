// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Journeys;

/// <summary>
/// A sequence of command shapes that things of one kind went through, and how many did.
/// </summary>
/// <param name="Kind">What the things are.</param>
/// <param name="Steps">The command shapes, in order.</param>
/// <param name="Entities">How many things have gone exactly this far.</param>
/// <param name="Share">Their share of all things of the kind, as a whole percentage.</param>
/// <param name="ContinuedBy">The step the most common longer sequence takes next, when others went further than these did; <see cref="CommandShape.None"/> when no one did.</param>
public record JourneySequence(EntityKind Kind, IEnumerable<CommandShape> Steps, int Entities, int Share, CommandShape ContinuedBy);
