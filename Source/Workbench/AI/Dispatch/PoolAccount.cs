// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// The spend waiting to be written.
/// </summary>
/// <param name="Tokens">The tokens.</param>
/// <param name="Calls">The answers they came from.</param>
sealed record PendingBatch(long Tokens, int Calls);

/// <summary>
/// What this process knows a pool used on one day: the tokens used and reserved, and the part of it not yet written to the event log.
/// All access is under one lock; it is held for arithmetic only, never across a call.
/// </summary>
sealed class PoolAccount
{
    readonly Lock _lock = new();
    bool _seeded;
    long _used;
    long _pending;
    int _pendingCalls;
    long _lastActual;
    DateTimeOffset _firstPending;

    /// <summary>
    /// Adds an estimate to the use. The first call seeds the account with what the read model reports; later calls take the larger of what is
    /// counted here and what the read model reports plus what it has not seen yet, so another writer's spend is not ignored.
    /// </summary>
    /// <param name="readModelUsed">What the read model says the pool used today.</param>
    /// <param name="defaultEstimate">The estimate when no answer has shown what a call costs.</param>
    /// <returns>What was used before the reservation and the estimate reserved.</returns>
    public (long UsedBefore, long Estimate) Reserve(long readModelUsed, long defaultEstimate)
    {
        lock (_lock)
        {
            _used = _seeded ? Math.Max(_used, readModelUsed + _pending) : readModelUsed;
            _seeded = true;
            var estimate = _lastActual > 0 ? _lastActual : defaultEstimate;
            var before = _used;
            _used += estimate;
            return (before, estimate);
        }
    }

    public void Release(long estimate)
    {
        lock (_lock)
        {
            _used = Math.Max(0, _used - estimate);
        }
    }

    public void Settle(long estimate, long actual, DateTimeOffset now)
    {
        lock (_lock)
        {
            _used = Math.Max(0, _used - estimate + actual);
            if (actual <= 0)
            {
                return;
            }

            _lastActual = actual;
            _firstPending = _pending == 0 ? now : _firstPending;
            _pending += actual;
            _pendingCalls++;
        }
    }

    public bool IsDue(DateTimeOffset now, TimeSpan maximumWait, int maximumCalls)
    {
        lock (_lock)
        {
            return _pending > 0 && (_pendingCalls >= maximumCalls || now - _firstPending >= maximumWait);
        }
    }

    public PendingBatch TakePending()
    {
        lock (_lock)
        {
            return new PendingBatch(_pending, _pendingCalls);
        }
    }

    public void Recorded(PendingBatch batch)
    {
        lock (_lock)
        {
            _pending = Math.Max(0, _pending - batch.Tokens);
            _pendingCalls = Math.Max(0, _pendingCalls - batch.Calls);
        }
    }
}
