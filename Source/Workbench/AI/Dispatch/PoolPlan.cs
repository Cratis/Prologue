// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Conversations;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Everything the dispatcher decides from when an agent's work arrives: its pool's members in the order to try them, the fallback
/// and the ceiling.
/// </summary>
/// <param name="PoolKey">The pool's id, which its arrangement, ceiling and spend are keyed by.</param>
/// <param name="Pool">The pool's name.</param>
/// <param name="Fallback">What the pool does when a member cannot take the work.</param>
/// <param name="Members">The members in the order to try them.</param>
/// <param name="Agent">Where the agent runs, to be narrowed to one member at a time.</param>
/// <param name="Ceiling">The most tokens the pool may use in a day; zero when it has no ceiling.</param>
/// <param name="Used">The tokens the pool used today.</param>
public record PoolPlan(string PoolKey, string Pool, PoolFallback Fallback, IReadOnlyList<DispatchMember> Members, ChatAgentTarget Agent, long Ceiling, long Used);
