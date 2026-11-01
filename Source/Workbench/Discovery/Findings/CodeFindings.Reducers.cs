// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discovery.Findings;

public partial record CodeFindings
{
    /// <summary>
    /// Reduces a capability being mapped.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue.</param>
    /// <returns>The new state.</returns>
    public static CodeFindings Reduce(CapabilityMapped @event, CodeFindings? current, EventContext context)
    {
        var state = current ?? Empty(new PrologueId(Guid.Parse(context.EventSourceId.Value)));
        var key = FindingKeys.Capability(@event.Module, @event.Capability);
        var mapped = new MappedCapability(@event.Subject, @event.Module, @event.Capability, [.. @event.Shapes], @event.File);
        return state with { Capabilities = [.. state.Capabilities.Where(_ => !Same(_, @event.Subject, key)), mapped] };
    }

    /// <summary>
    /// Reduces a capability being removed.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static CodeFindings? Reduce(CapabilityRemoved @event, CodeFindings? current)
    {
        var key = FindingKeys.Capability(@event.Module, @event.Capability);
        return current is null ? null : current with { Capabilities = [.. current.Capabilities.Where(_ => !Same(_, @event.Subject, key))] };
    }

    /// <summary>
    /// Reduces a decision point being found.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue.</param>
    /// <returns>The new state.</returns>
    public static CodeFindings Reduce(DecisionPointFound @event, CodeFindings? current, EventContext context)
    {
        var state = current ?? Empty(new PrologueId(Guid.Parse(context.EventSourceId.Value)));
        var key = FindingKeys.DecisionPoint(@event.Rule, @event.File);
        var found = new MappedDecisionPoint(@event.Subject, @event.Rule, @event.File, @event.Line, @event.Shape, @event.Kind);
        return state with { DecisionPoints = [.. state.DecisionPoints.Where(_ => !Same(_, @event.Subject, key)), found] };
    }

    /// <summary>
    /// Reduces a decision point being removed.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static CodeFindings? Reduce(DecisionPointRemoved @event, CodeFindings? current)
    {
        var key = FindingKeys.DecisionPoint(@event.Rule, @event.File);
        return current is null ? null : current with { DecisionPoints = [.. current.DecisionPoints.Where(_ => !Same(_, @event.Subject, key))] };
    }

    /// <summary>
    /// Reduces a dependency being found.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue.</param>
    /// <returns>The new state.</returns>
    public static CodeFindings Reduce(DependencyFound @event, CodeFindings? current, EventContext context)
    {
        var state = current ?? Empty(new PrologueId(Guid.Parse(context.EventSourceId.Value)));
        var key = FindingKeys.Dependency(@event.Target, @event.Kind);
        var found = new MappedDependency(@event.Subject, @event.Target, @event.Kind, @event.Detail);
        return state with { Dependencies = [.. state.Dependencies.Where(_ => !Same(_, @event.Subject, key)), found] };
    }

    /// <summary>
    /// Reduces a dependency being removed.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The new state.</returns>
    public static CodeFindings? Reduce(DependencyRemoved @event, CodeFindings? current)
    {
        var key = FindingKeys.Dependency(@event.Target, @event.Kind);
        return current is null ? null : current with { Dependencies = [.. current.Dependencies.Where(_ => !Same(_, @event.Subject, key))] };
    }

    /// <summary>
    /// Reduces an analysis completing.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue.</param>
    /// <returns>The new state.</returns>
    public static CodeFindings Reduce(CodeAnalysisCompleted @event, CodeFindings? current, EventContext context)
    {
        var state = current ?? Empty(new PrologueId(Guid.Parse(context.EventSourceId.Value)));
        var run = new AnalysisRun(@event.Subject, @event.Name, @event.Source, @event.Files, null, context.Occurred);
        return state with { Runs = [.. state.Runs.Where(_ => _.Subject != @event.Subject), run] };
    }

    /// <summary>
    /// Reduces an analysis failing, which keeps what the last complete analysis read.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event, which carries the prologue.</param>
    /// <returns>The new state.</returns>
    public static CodeFindings Reduce(CodeAnalysisFailed @event, CodeFindings? current, EventContext context)
    {
        var state = current ?? Empty(new PrologueId(Guid.Parse(context.EventSourceId.Value)));
        var previous = state.RunOf(@event.Subject);
        var run = new AnalysisRun(@event.Subject, @event.Name, previous?.Source, previous?.Files ?? new(0), @event.Reason, context.Occurred);
        return state with { Runs = [.. state.Runs.Where(_ => _.Subject != @event.Subject), run] };
    }

    static bool Same(MappedCapability item, AnalysisSubjectId subject, string key) =>
        item.Subject == subject && FindingKeys.Capability(item.Module, item.Capability) == key;

    static bool Same(MappedDecisionPoint item, AnalysisSubjectId subject, string key) =>
        item.Subject == subject && FindingKeys.DecisionPoint(item.Rule, item.File) == key;

    static bool Same(MappedDependency item, AnalysisSubjectId subject, string key) =>
        item.Subject == subject && FindingKeys.Dependency(item.Target, item.Kind) == key;
}
