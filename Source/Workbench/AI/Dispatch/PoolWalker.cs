// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Tries a pool's members in order under the pool's fallback. It knows nothing about vendors: what to do with a member is the attempt it is given.
/// </summary>
public static class PoolWalker
{
    /// <summary>
    /// Enforces the pool's ceiling, then walks the members until one takes the work. A member that cannot start, or whose attempt fails, is
    /// passed over when the pool continues on the next provider. A pool that pauses and asks also moves on from a member
    /// inside the network to another inside it, but never to one outside it.
    /// </summary>
    /// <typeparam name="T">What an attempt produces.</typeparam>
    /// <param name="plan">The <see cref="PoolPlan"/>.</param>
    /// <param name="attempt">Hands the work to one member.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>What the first member that took the work produced.</returns>
    /// <exception cref="PoolCeilingReached">The pool has used everything its ceiling allows today.</exception>
    /// <exception cref="AgentDispatchPaused">The pool paused before using a provider outside the network.</exception>
    /// <exception cref="PoolExhausted">No member could take the work.</exception>
    public static async Task<T> Walk<T>(PoolPlan plan, Func<DispatchMember, Task<T>> attempt, CancellationToken cancellationToken)
    {
        if (plan.Ceiling > 0 && plan.Used >= plan.Ceiling)
        {
            throw new PoolCeilingReached(plan.Pool, plan.Ceiling, plan.Used);
        }

        var reasons = new List<string>();
        string? insideFailure = null;
        foreach (var member in plan.Members)
        {
            if (plan.Fallback == PoolFallback.PauseAndAsk && !member.IsInside && insideFailure is not null)
            {
                throw new AgentDispatchPaused(plan.Pool, insideFailure, member.Name);
            }

            var failure = member.CanStart ? await TryAttempt(member, attempt, cancellationToken) : ((Holder<T>?)null, member.Problem);
            if (failure.Item1 is not null)
            {
                return failure.Item1.Value;
            }

            reasons.Add($"{member.Name}: {failure.Item2}");
            if (member.IsInside)
            {
                insideFailure ??= member.Name;
            }
        }

        throw new PoolExhausted(plan.Pool, reasons);
    }

    static async Task<(Holder<T>?, string)> TryAttempt<T>(DispatchMember member, Func<DispatchMember, Task<T>> attempt, CancellationToken cancellationToken)
    {
        try
        {
            return (new Holder<T>(await attempt(member)), string.Empty);
        }
        catch (Exception exception) when (exception is not (OperationCanceledException or AgentDispatchStopped))
        {
            return (null, string.IsNullOrWhiteSpace(exception.Message) ? "could not be reached" : exception.Message);
        }
    }

    sealed record Holder<T>(T Value);
}
