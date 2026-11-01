// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Systems;
using Cratis.Prologue.Workbench.Systems.Adding;
using Cratis.Prologue.Workbench.Systems.Locating;
using Cratis.Prologue.Workbench.Systems.Removing;
using Cratis.Prologue.Workbench.Systems.Renaming;
using Cratis.Prologue.Workbench.Systems.RuntimeSources;

namespace Cratis.Prologue.Workbench.Prologues;

public partial record PrologueDefinition
{
    /// <summary>
    /// Reduces a system being added.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(SystemAdded @event, PrologueDefinition? current) =>
        current is null ? null : current with { Systems = [.. current.Systems, new PrologueSystem(@event.Id, @event.Name, null, [], [], [], [])] };

    /// <summary>
    /// Reduces a system being renamed.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(SystemRenamed @event, PrologueDefinition? current) =>
        SystemChange.Apply(current, @event.Id, system => system with { Name = @event.Name });

    /// <summary>
    /// Reduces a system being removed.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(SystemRemoved @event, PrologueDefinition? current) =>
        current is null ? null : current with { Systems = [.. current.Systems.Where(system => system.Id != @event.Id)] };

    /// <summary>
    /// Reduces a system's code being located.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(SystemCodeLocated @event, PrologueDefinition? current) =>
        SystemChange.Apply(current, @event.Id, system => system with { Location = @event.ToLocation() });

    /// <summary>
    /// Reduces a system's HTTP clusters being set.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(SystemHttpClustersSet @event, PrologueDefinition? current) =>
        SystemChange.Apply(current, @event.Id, system => system with { HttpClusters = @event.Clusters });

    /// <summary>
    /// Reduces a system's databases being set.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(SystemDatabaseSourcesSet @event, PrologueDefinition? current) =>
        SystemChange.Apply(current, @event.Id, system => system with { Databases = @event.Sources });

    /// <summary>
    /// Reduces a system's telemetry being set.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static PrologueDefinition? Reduce(SystemTelemetrySet @event, PrologueDefinition? current) =>
        SystemChange.Apply(current, @event.Id, system => system with { TelemetryServices = @event.Services, AttributeKeys = @event.AttributeKeys });
}
