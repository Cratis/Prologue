// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues.Correlation;
using Cratis.Prologue.Workbench.Prologues.Defining;
using Cratis.Prologue.Workbench.Prologues.Evidence;
using Cratis.Prologue.Workbench.Prologues.Output;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Prologues;

/// <summary>
/// A prologue as it is defined: its evidence steps, how captures are correlated and delivered, and its systems.
/// </summary>
/// <param name="Id">The identity of the prologue.</param>
/// <param name="Name">What the prologue is called.</param>
/// <param name="Steps">Which evidence steps it gathers.</param>
/// <param name="Correlation">The correlation window.</param>
/// <param name="Output">Where captures are sent.</param>
/// <param name="Systems">The systems it works on.</param>
[ReadModel]
public partial record PrologueDefinition(
    PrologueId Id,
    PrologueName Name,
    EvidenceSteps Steps,
    CorrelationWindow Correlation,
    CaptureOutput Output,
    IEnumerable<PrologueSystem> Systems)
{
    /// <summary>
    /// Observes every prologue, so the shell can offer them.
    /// </summary>
    /// <param name="collection">The projected prologues.</param>
    /// <returns>The prologues.</returns>
    public static ISubject<IEnumerable<PrologueDefinition>> ObservePrologues(IMongoCollection<PrologueDefinition> collection) =>
        collection.Observe();

    /// <summary>
    /// Observes one prologue.
    /// </summary>
    /// <param name="id">The prologue to observe.</param>
    /// <param name="collection">The collection the prologue is read from.</param>
    /// <returns>The prologue.</returns>
    public static ISubject<PrologueDefinition> ObservePrologue(PrologueId id, IMongoCollection<PrologueDefinition> collection) =>
        collection.ObserveById<PrologueDefinition, PrologueId>(id);

    /// <summary>
    /// Reduces a prologue being defined.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition Reduce(PrologueDefined @event, PrologueDefinition? current, EventContext context) =>
        current ?? new(
            new PrologueId(Guid.Parse(context.EventSourceId.Value)),
            @event.Name,
            EvidenceSteps.All,
            CorrelationWindow.Default,
            CaptureOutput.Api,
            []);

    /// <summary>
    /// Reduces an evidence step being turned on.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(EvidenceStepTurnedOn @event, PrologueDefinition? current) =>
        current is null ? null : current with { Steps = current.Steps.With(@event.Step, true) };

    /// <summary>
    /// Reduces an evidence step being turned off.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(EvidenceStepTurnedOff @event, PrologueDefinition? current) =>
        current is null ? null : current with { Steps = current.Steps.With(@event.Step, false) };

    /// <summary>
    /// Reduces the correlation window being set.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(CorrelationWindowSet @event, PrologueDefinition? current) =>
        current is null ? null : current with { Correlation = @event.Window };

    /// <summary>
    /// Reduces the output being chosen.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(OutputChosen @event, PrologueDefinition? current) =>
        current is null ? null : current with { Output = @event.Kind };
}
