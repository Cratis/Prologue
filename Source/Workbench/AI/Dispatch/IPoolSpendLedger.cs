// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Holds, for this process, how much each pool has used today, so the ceiling is held between calls that run at the same time and the spend
/// reaches the event log in batches.
/// </summary>
public interface IPoolSpendLedger
{
    /// <summary>
    /// Reserves an estimate of what a call will cost before it is made. The reservation is added to the pool's use at once, so calls that start
    /// together cannot all pass the ceiling.
    /// </summary>
    /// <param name="plan">The plan of the call, which carries the pool's ceiling and what the read model says it used.</param>
    /// <returns>The reservation, which the caller settles with the actual usage or disposes to give the estimate back.</returns>
    PoolReservation Reserve(PoolPlan plan);

    /// <summary>
    /// Records the spend that has waited long enough, or has grown large enough, to be written.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A task for the asynchronous operation.</returns>
    Task FlushDue(CancellationToken cancellationToken);

    /// <summary>
    /// Records all spend that is waiting, for example on shutdown.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A task for the asynchronous operation.</returns>
    Task FlushAll(CancellationToken cancellationToken);
}
