// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery;

/// <summary>
/// What a decision point in the code does when it decides.
/// </summary>
public enum DecisionKind
{
    /// <summary>
    /// Refuses a command: the runtime evidence for it is the command being answered with a client error.
    /// </summary>
    Refusal,

    /// <summary>
    /// Changes state when a command is accepted: the runtime evidence is the command succeeding.
    /// </summary>
    StateChange,

    /// <summary>
    /// Runs on a schedule, outside any command: the runtime evidence is database writes no command explains.
    /// </summary>
    Schedule
}
