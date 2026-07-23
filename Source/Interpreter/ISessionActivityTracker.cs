// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Defines the tracker the session grain and HTTP endpoints report activity to, and that the lifecycle watcher
/// asks whether the service should exit. Status polling deliberately does not count as activity — otherwise a
/// consumer polling for progress would keep an abandoned service alive forever.
/// </summary>
public interface ISessionActivityTracker
{
    /// <summary>
    /// Records that a session was interacted with — started or otherwise driven forward.
    /// </summary>
    void SessionTouched();

    /// <summary>
    /// Records a session status transition — entering <see cref="InterpreterStatus.AwaitingAnswers"/> starts the
    /// grace-period countdown, any other status cancels it, and a terminal status marks the session finished.
    /// </summary>
    /// <param name="status">The status the session transitioned to.</param>
    void StatusChanged(InterpreterStatus status);

    /// <summary>
    /// Records that an answer arrived — resets the grace-period countdown while the session awaits more answers.
    /// </summary>
    void AnswerReceived();

    /// <summary>
    /// Records that the session's result was fetched.
    /// </summary>
    void ResultFetched();

    /// <summary>
    /// Evaluates whether the service should exit cleanly.
    /// </summary>
    /// <param name="gracePeriod">How long the service waits for answers while a session is parked awaiting them.</param>
    /// <param name="idleTimeout">How long the service lingers without session activity.</param>
    /// <returns>The <see cref="ShutdownReason"/> calling for an exit, or <see cref="ShutdownReason.None"/>.</returns>
    ShutdownReason Evaluate(TimeSpan gracePeriod, TimeSpan idleTimeout);
}
