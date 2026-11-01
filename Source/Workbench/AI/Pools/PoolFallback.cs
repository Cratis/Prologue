// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Pools;

/// <summary>
/// What a provider pool does when the member it tried cannot take the work.
/// </summary>
public enum PoolFallback
{
    /// <summary>
    /// Continue on the next provider in the pool.
    /// </summary>
    ContinueOnNext = 0,

    /// <summary>
    /// Pause and ask a person before falling back to another provider.
    /// </summary>
    PauseAndAsk = 1,
}
