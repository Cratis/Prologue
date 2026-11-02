// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.AI.Asking;

/// <summary>
/// What an agent answered when asked, or why it did not.
/// </summary>
/// <param name="Succeeded">Whether the agent produced an answer.</param>
/// <param name="Text">The answer; empty when it did not succeed.</param>
/// <param name="Reason">Why there is no answer; empty when there is one.</param>
/// <param name="Stopped">Why the dispatcher did not hand the work on, when it deliberately did not; otherwise null.</param>
public record AgentReply(bool Succeeded, string Text, string Reason, Dispatch.AgentDispatchOutcome? Stopped = null)
{
    /// <summary>
    /// Creates the reply of an agent that answered.
    /// </summary>
    /// <param name="text">The answer.</param>
    /// <returns>The reply.</returns>
    public static AgentReply Answered(string text) => new(true, text, string.Empty);

    /// <summary>
    /// Creates the reply of an agent that could not answer.
    /// </summary>
    /// <param name="reason">Why, in words a person can act on.</param>
    /// <returns>The reply.</returns>
    public static AgentReply Unavailable(string reason) => new(false, string.Empty, reason);

    /// <summary>
    /// Creates the reply of an agent the dispatcher deliberately did not run: its pool paused, reached its ceiling or had no provider left.
    /// </summary>
    /// <param name="stop">The stop, whose message a person can act on.</param>
    /// <returns>The reply.</returns>
    public static AgentReply StoppedBy(Dispatch.AgentDispatchStopped stop) => new(false, string.Empty, stop.Message, stop.Outcome);
}
