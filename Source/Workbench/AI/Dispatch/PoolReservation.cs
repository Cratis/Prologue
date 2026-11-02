// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// What a call to a pool has set aside against the pool's ceiling. It ends exactly once: settled with the usage the call reported,
/// or given back when the call produced nothing.
/// </summary>
/// <param name="usedBefore">What the pool had used, counting every call still running, before this reservation.</param>
/// <param name="settle">Replaces the estimate with the actual tokens.</param>
/// <param name="release">Gives the estimate back.</param>
public sealed class PoolReservation(long usedBefore, Action<long> settle, Action release) : IDisposable
{
    int _ended;

    /// <summary>
    /// Gets what the pool had used, counting every call still running, before this reservation.
    /// </summary>
    public long UsedBefore { get; } = usedBefore;

    /// <summary>
    /// Ends the reservation with what the call used. Only the first end counts.
    /// </summary>
    /// <param name="usage">The usage the provider reported, if it did.</param>
    public void Settle(UsageDetails? usage)
    {
        if (Interlocked.Exchange(ref _ended, 1) == 0)
        {
            settle(PoolSpendRecorder.TokensOf(usage));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _ended, 1) == 0)
        {
            release();
        }
    }
}
