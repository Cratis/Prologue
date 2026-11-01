// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Where an interpretation of a prologue is.
/// </summary>
public enum InterpretationState
{
    /// <summary>
    /// Nothing is running and nothing has been run since the Receiver started.
    /// </summary>
    Idle,

    /// <summary>
    /// An interpretation is waiting for its turn.
    /// </summary>
    Queued,

    /// <summary>
    /// The captures are being read and modeled.
    /// </summary>
    Running,

    /// <summary>
    /// The interpreter asked questions and waits for the answers.
    /// </summary>
    AwaitingAnswers,

    /// <summary>
    /// The interpretation finished and its result was recorded.
    /// </summary>
    Completed,

    /// <summary>
    /// The interpretation could not finish.
    /// </summary>
    Failed
}
