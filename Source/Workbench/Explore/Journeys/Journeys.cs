// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.Journeys;

/// <summary>
/// The journeys things go through, inferred from the identities in request paths and kept up to date as captures arrive. It
/// follows the commands captured for each thing (a loan, a book) and counts how many things went through each sequence.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Trails">The things being followed, the one touched last at the end. Bounded by <see cref="JourneyInference.TrailsKept"/>.</param>
/// <param name="Sequences">The sequences, described: most common first within each kind.</param>
[ReadModel]
public record Journeys(PrologueId Id, IEnumerable<EntityTrail> Trails, IEnumerable<JourneySequence> Sequences)
{
    /// <summary>
    /// Observes the journeys of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored journeys.</param>
    /// <returns>The journeys, pushed again whenever a capture extends one.</returns>
    public static ISubject<Journeys> ObserveJourneys(PrologueId id, IReadModels readModels) =>
        readModels.Observe<Journeys>(journeys => journeys.Id == id).FirstItem();

    /// <summary>
    /// Reduces a capture arriving.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static Journeys Reduce(CaptureRecorded @event, Journeys? current, EventContext context)
    {
        var journeys = current ?? new(new PrologueId(Guid.Parse(context.EventSourceId.Value)), [], []);
        if (!JourneyInference.IsStep(@event.Http))
        {
            return journeys;
        }

        var (trails, sequences) = JourneyInference.Link(journeys.Trails, journeys.Sequences, @event.Http);
        return journeys with { Trails = trails, Sequences = JourneyInference.Describe(sequences) };
    }
}
