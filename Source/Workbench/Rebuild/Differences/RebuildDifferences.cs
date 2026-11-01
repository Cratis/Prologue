// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Rebuild.Parity;

namespace Cratis.Prologue.Workbench.Rebuild.Differences;

/// <summary>
/// The requests the legacy system and the new one decided differently, newest first. Every open difference is kept; only resolved
/// ones beyond <see cref="ResolvedKept"/> fall off, so a difference that blocks a cut over can always be resolved.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Differences">The differences, newest first.</param>
[ReadModel]
public record RebuildDifferences(PrologueId Id, IEnumerable<RebuildDifference> Differences)
{
    /// <summary>
    /// The most resolved differences kept.
    /// </summary>
    public const int ResolvedKept = 100;

    /// <summary>
    /// Observes the differences of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored differences.</param>
    /// <returns>The differences, pushed again whenever one is found or resolved.</returns>
    public static ISubject<RebuildDifferences> ObserveRebuildDifferences(PrologueId id, IReadModels readModels) =>
        readModels.Observe<RebuildDifferences>(differences => differences.Id == id).FirstItem();

    /// <summary>
    /// Reduces an observation: a mismatch becomes an open difference, anything else is not a difference.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildDifferences Reduce(ParityObserved @event, RebuildDifferences? current, EventContext context)
    {
        var differences = RebuildBoards.Existing(current, context);
        if (@event.Outcome != ParityOutcome.Mismatched)
        {
            return differences;
        }

        return (differences with { Differences = [RebuildDifference.From(@event), .. differences.Differences] }).Trimmed();
    }

    /// <summary>
    /// Reduces a difference being accepted.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildDifferences Reduce(DifferenceAccepted @event, RebuildDifferences? current, EventContext context) =>
        RebuildBoards.Existing(current, context).Resolved(@event.Observation, DifferenceResolution.Accepted);

    /// <summary>
    /// Reduces a difference being marked to be matched to the legacy behavior.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static RebuildDifferences Reduce(DifferenceToBeMatched @event, RebuildDifferences? current, EventContext context) =>
        RebuildBoards.Existing(current, context).Resolved(@event.Observation, DifferenceResolution.ToBeMatched);

    /// <summary>
    /// Gets a difference that has not been resolved.
    /// </summary>
    /// <param name="differences">The differences, or null when the prologue has none.</param>
    /// <param name="observation">The observation.</param>
    /// <returns>The open difference, or null when there is none.</returns>
    public static RebuildDifference? Open(RebuildDifferences? differences, ObservationId observation) =>
        differences?.Differences.FirstOrDefault(difference => difference.Observation == observation && difference.Resolution == DifferenceResolution.Open);

    RebuildDifferences Resolved(ObservationId observation, DifferenceResolution resolution) =>
        (this with { Differences = [.. Differences.Select(difference => difference.Observation == observation ? difference with { Resolution = resolution } : difference)] }).Trimmed();

    RebuildDifferences Trimmed()
    {
        var resolvedSeen = 0;
        return this with { Differences = [.. Differences.Where(difference => difference.Resolution == DifferenceResolution.Open || ++resolvedSeen <= ResolvedKept)] };
    }
}
