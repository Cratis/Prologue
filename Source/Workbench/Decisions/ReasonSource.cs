// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// Where a decision's reason comes from.
/// </summary>
public enum ReasonSource
{
    /// <summary>
    /// The reason is a pattern in what was captured, such as nothing being written.
    /// </summary>
    Observed,

    /// <summary>
    /// The reason is a decision point Discover found in the code.
    /// </summary>
    Discovered
}
