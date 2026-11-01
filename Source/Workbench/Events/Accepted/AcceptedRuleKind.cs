// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// Where a rule of the accepted model comes from.
/// </summary>
public enum AcceptedRuleKind
{
    /// <summary>
    /// A rule the evidence suggests, for example from refusals or the code.
    /// </summary>
    Rule,

    /// <summary>
    /// A constraint the evidence suggests, for example from a unique index.
    /// </summary>
    Constraint,

    /// <summary>
    /// The way a person chose to hold a consistency boundary.
    /// </summary>
    Boundary
}
