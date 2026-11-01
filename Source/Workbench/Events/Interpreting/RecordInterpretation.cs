// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Events.Boundaries;
using Cratis.Prologue.Workbench.Events.NotEvents;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Records what an interpretation found. Only what differs from the last interpretation becomes an event, so interpreting the same evidence again
/// records nothing. The runner executes this command; it is not for people.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Suggestions">The events suggested.</param>
/// <param name="Model">The structure around them.</param>
/// <param name="Boundaries">The consistency boundaries the evidence raises.</param>
/// <param name="NotEvents">What the evidence shows is not an event.</param>
[Command]
public record RecordInterpretation(
    PrologueId Prologue,
    IEnumerable<SuggestedEvent> Suggestions,
    InterpretedModel Model,
    IEnumerable<ConsistencyBoundary> Boundaries,
    IEnumerable<NotEvent> NotEvents) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Prologue;

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="current">The review as it is now.</param>
    /// <returns>The events that differ from the last interpretation.</returns>
    public IEnumerable<object> Handle([Unprotected("Interpretations run one at a time; comparing with a read model that trails the log costs at most one repeated fact, never a lost decision")] EventReview? current) =>
        InterpretationDecision.Decide(current, Suggestions, Model, Boundaries, NotEvents);
}
