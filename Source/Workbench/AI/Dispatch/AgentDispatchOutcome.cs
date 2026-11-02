// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Why the dispatcher did not hand an agent's work to a provider.
/// </summary>
public enum AgentDispatchOutcome
{
    /// <summary>
    /// The pool paused before moving from a provider inside the network to one outside it.
    /// </summary>
    Paused = 0,

    /// <summary>
    /// The pool has used everything its ceiling allows for today.
    /// </summary>
    CeilingReached = 1,

    /// <summary>
    /// Every provider the pool was allowed to try failed.
    /// </summary>
    Exhausted = 2,
}
