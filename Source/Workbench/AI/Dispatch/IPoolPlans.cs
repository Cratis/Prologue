// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.LanguageModels;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Works out, when an agent's work arrives, the pool it draws from and what the Workbench recorded about that pool.
/// </summary>
public interface IPoolPlans
{
    /// <summary>
    /// Plans the dispatch for the agent behind a purpose.
    /// </summary>
    /// <param name="purpose">The purpose, the agent's identity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The plan; null only when the agent draws from no pool at all, which Cratis.AI then dispatches as it always did.</returns>
    /// <exception cref="PoolNotReady">The agent draws from a pool that cannot be read.</exception>
    Task<PoolPlan?> For(LanguageModelPurpose purpose, CancellationToken cancellationToken);
}
