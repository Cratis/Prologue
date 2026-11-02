// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Records what an answer cost a pool, so the pool's ceiling can be held.
/// </summary>
public interface IPoolSpendRecorder
{
    /// <summary>
    /// Records the tokens an answer used against a pool. A usage the provider did not report records nothing, and a failure to record never fails the answer.
    /// </summary>
    /// <param name="poolKey">The pool's id.</param>
    /// <param name="usage">The usage the provider reported, if it did.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A task for the asynchronous operation.</returns>
    Task Record(string poolKey, UsageDetails? usage, CancellationToken cancellationToken);
}
