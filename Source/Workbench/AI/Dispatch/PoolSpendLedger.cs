// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// The <see cref="IPoolSpendLedger"/>: one account per pool and UTC day, seeded from the spend the read model reports the first time the pool is used.
/// </summary>
/// <param name="recorder">Writes a batch of spend to the event log.</param>
/// <param name="time">Tells the day and how long spend has waited.</param>
/// <param name="logger">The logger.</param>
public class PoolSpendLedger(IPoolSpendRecorder recorder, TimeProvider time, ILogger<PoolSpendLedger> logger) : IPoolSpendLedger, IDisposable
{
    /// <summary>
    /// What a call is assumed to cost before the pool has shown what one does.
    /// </summary>
    public const long DefaultEstimate = 2000;

    /// <summary>
    /// The number of answers after which spend is written without waiting.
    /// </summary>
    public const int MaximumCalls = 50;

    /// <summary>
    /// The longest spend waits before it is written.
    /// </summary>
    public static readonly TimeSpan MaximumWait = TimeSpan.FromSeconds(30);

    readonly ConcurrentDictionary<(string Pool, string Day), PoolAccount> _accounts = new();
    readonly SemaphoreSlim _flushing = new(1, 1);

    /// <inheritdoc/>
    public PoolReservation Reserve(PoolPlan plan)
    {
        var day = PoolPlans.DayOf(time);
        var account = _accounts.GetOrAdd((plan.PoolKey, day), _ => new PoolAccount());
        var (usedBefore, estimate) = account.Reserve(plan.Used, DefaultEstimate);
        return new PoolReservation(
            usedBefore,
            actual =>
            {
                account.Settle(estimate, actual, time.GetUtcNow());
                if (account.IsDue(time.GetUtcNow(), MaximumWait, MaximumCalls))
                {
                    _ = FlushDue(CancellationToken.None);
                }
            },
            () => account.Release(estimate));
    }

    /// <inheritdoc/>
    public void Dispose() => _flushing.Dispose();

    /// <inheritdoc/>
    public Task FlushDue(CancellationToken cancellationToken) => Flush(force: false, cancellationToken);

    /// <inheritdoc/>
    public Task FlushAll(CancellationToken cancellationToken) => Flush(force: true, cancellationToken);

    async Task Flush(bool force, CancellationToken cancellationToken)
    {
        await _flushing.WaitAsync(cancellationToken);
        try
        {
            var now = time.GetUtcNow();
            foreach (var ((pool, day), account) in _accounts)
            {
                if (!force && !account.IsDue(now, MaximumWait, MaximumCalls))
                {
                    continue;
                }

                var batch = account.TakePending();
                if (batch.Tokens <= 0)
                {
                    continue;
                }

                if (await recorder.Record(pool, day, batch.Tokens, cancellationToken))
                {
                    account.Recorded(batch);
                }
                else
                {
                    logger.SpendKeptForLater(pool);
                }
            }
        }
        finally
        {
            _flushing.Release();
        }
    }
}
