// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Runtime;

/// <summary>
/// How a decision point in the code stands against the runtime evidence.
/// </summary>
public enum DecisionRuntime
{
    /// <summary>
    /// It happened at runtime.
    /// </summary>
    Matches,

    /// <summary>
    /// It was never exercised: run a scenario to see it.
    /// </summary>
    CodeOnly,

    /// <summary>
    /// A scheduled job in the code explains database writes that no command does.
    /// </summary>
    ExplainsWrites
}
