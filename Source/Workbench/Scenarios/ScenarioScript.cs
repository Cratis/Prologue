// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios.Marking;
using Cratis.Prologue.Workbench.Scenarios.Scripting;

namespace Cratis.Prologue.Workbench.Scenarios;

/// <summary>
/// The scenarios a prologue's captures are labeled with, and the one that is running now. It only follows the events about
/// scenarios, never the captures, so labeling a capture never competes with changing the script.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Scenarios">The scenarios to run, in the order they were added.</param>
/// <param name="Active">The scenario that is running, or <see cref="ScenarioName.None"/>.</param>
[ReadModel]
public record ScenarioScript(PrologueId Id, IEnumerable<ScenarioName> Scenarios, ScenarioName Active)
{
    /// <summary>
    /// Observes the scenario script of a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored scripts.</param>
    /// <returns>The script.</returns>
    public static ISubject<ScenarioScript> ObserveScenarioScript(PrologueId id, IReadModels readModels) =>
        readModels.Observe<ScenarioScript>(script => script.Id == id).FirstItem();

    /// <summary>
    /// Reduces a scenario being added to the script.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static ScenarioScript Reduce(ScenarioAdded @event, ScenarioScript? current, EventContext context) =>
        ScenarioScripts.Existing(current, context).WithScenario(@event.Name);

    /// <summary>
    /// Reduces a scenario being removed from the script.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static ScenarioScript Reduce(ScenarioRemoved @event, ScenarioScript? current, EventContext context)
    {
        var script = ScenarioScripts.Existing(current, context);
        return script with { Scenarios = script.Scenarios.Where(name => name != @event.Name) };
    }

    /// <summary>
    /// Reduces a scenario starting. A scenario typed in rather than picked joins the script.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static ScenarioScript Reduce(ScenarioStarted @event, ScenarioScript? current, EventContext context) =>
        ScenarioScripts.Existing(current, context).WithScenario(@event.Name) with { Active = @event.Name };

    /// <summary>
    /// Reduces a scenario ending.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue's identity.</param>
    /// <returns>The new state.</returns>
    public static ScenarioScript Reduce(ScenarioEnded @event, ScenarioScript? current, EventContext context) =>
        ScenarioScripts.Existing(current, context) with { Active = ScenarioName.None };

    ScenarioScript WithScenario(ScenarioName name) =>
        Scenarios.Contains(name) ? this : this with { Scenarios = [.. Scenarios, name] };
}
