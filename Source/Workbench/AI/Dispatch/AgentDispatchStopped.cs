// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// The exception that is thrown when the dispatcher deliberately does not hand an agent's work on, with a message a person can act on.
/// Callers show the message in their status text; <see cref="Outcome"/> says which kind of stop it was.
/// </summary>
/// <param name="outcome">The <see cref="AgentDispatchOutcome"/>.</param>
/// <param name="pool">The name of the pool that stopped.</param>
/// <param name="message">What happened and what to do about it.</param>
public abstract class AgentDispatchStopped(AgentDispatchOutcome outcome, string pool, string message) : Exception(message)
{
    /// <summary>
    /// Gets which kind of stop it was.
    /// </summary>
    public AgentDispatchOutcome Outcome { get; } = outcome;

    /// <summary>
    /// Gets the name of the pool that stopped.
    /// </summary>
    public string Pool { get; } = pool;
}

/// <summary>
/// The exception that is thrown when a pool set to pause and ask stops before using a provider outside the network.
/// </summary>
/// <param name="pool">The name of the pool.</param>
/// <param name="failed">The name of the provider inside the network that could not take the work.</param>
/// <param name="next">The name of the provider outside the network that would have been used next.</param>
public class AgentDispatchPaused(string pool, string failed, string next) : AgentDispatchStopped(
    AgentDispatchOutcome.Paused,
    pool,
    $"The {pool} pool paused before sending work outside your network: {failed} could not take it and {next} is next. Allow it on the Systems page or switch the pool to continue on the next provider.")
{
    /// <summary>
    /// Gets the name of the provider inside the network that could not take the work.
    /// </summary>
    public string Failed { get; } = failed;

    /// <summary>
    /// Gets the name of the provider outside the network that would have been used next.
    /// </summary>
    public string Next { get; } = next;
}

/// <summary>
/// The exception that is thrown when a pool has used all its ceiling allows for the day.
/// </summary>
/// <param name="pool">The name of the pool.</param>
/// <param name="ceiling">The ceiling, in tokens per day.</param>
/// <param name="used">The tokens used today.</param>
public class PoolCeilingReached(string pool, long ceiling, long used) : AgentDispatchStopped(
    AgentDispatchOutcome.CeilingReached,
    pool,
    $"The {pool} pool reached its ceiling of {ceiling.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)} tokens for today ({used.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)} used). It starts over at midnight UTC; raise the ceiling on the Systems page to continue sooner.")
{
    /// <summary>
    /// Gets the ceiling, in tokens per day.
    /// </summary>
    public long Ceiling { get; } = ceiling;

    /// <summary>
    /// Gets the tokens used today.
    /// </summary>
    public long Used { get; } = used;
}

/// <summary>
/// The exception that is thrown when no provider the pool may use could take the work.
/// </summary>
/// <param name="pool">The name of the pool.</param>
/// <param name="reasons">Why each provider that was tried could not take the work.</param>
public class PoolExhausted(string pool, IReadOnlyList<string> reasons) : AgentDispatchStopped(
    AgentDispatchOutcome.Exhausted,
    pool,
    reasons.Count == 0
        ? $"The {pool} pool has no providers. Add one on the Systems page."
        : $"No provider in the {pool} pool could take the work. {string.Join("; ", reasons)}")
{
    /// <summary>
    /// Gets why each provider that was tried could not take the work.
    /// </summary>
    public IReadOnlyList<string> Reasons { get; } = reasons;
}

/// <summary>
/// The exception that is thrown when an agent is set to draw from a pool the dispatcher cannot read yet, for example while the pool is still being
/// projected or after it was deleted. The work stops rather than going around the pool's order, pause and ceiling.
/// </summary>
/// <param name="agent">The name of the agent.</param>
/// <param name="pool">The id of the pool, the only thing known of it.</param>
public class PoolNotReady(string agent, string pool) : AgentDispatchStopped(
    AgentDispatchOutcome.NotReady,
    pool,
    $"The pool {agent} draws from is not ready yet, so {agent} was not run. Try again in a moment; if it keeps happening, pick a pool for {agent} on the Systems page.");
