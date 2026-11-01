// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// What the observed system decided about a command.
/// </summary>
public enum DecisionOutcome
{
    /// <summary>
    /// The capture holds no command, or an answer that is neither a success nor a refusal, so nothing was decided.
    /// </summary>
    NotDecided,

    /// <summary>
    /// The system accepted the command: it answered with a success or a redirect.
    /// </summary>
    Accepted,

    /// <summary>
    /// The system refused the command: it answered with a client or server error.
    /// </summary>
    Refused
}
