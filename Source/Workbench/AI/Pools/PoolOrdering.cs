// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Pools;

/// <summary>
/// The order a provider pool tries its members in.
/// </summary>
public enum PoolOrdering
{
    /// <summary>
    /// The member with the most capacity left goes first.
    /// </summary>
    ByRemainingCapacity = 0,

    /// <summary>
    /// The members are tried in the order a person put them in.
    /// </summary>
    InPriorityOrder = 1,
}
