// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents why the interpreter service decides to exit cleanly — the session state is persisted, so exiting is
/// always safe and the orchestrator restarts the container later to resume.
/// </summary>
public enum ShutdownReason
{
    /// <summary>
    /// Nothing calls for a shutdown — the service keeps running.
    /// </summary>
    None,

    /// <summary>
    /// A session has been parked awaiting answers for longer than the grace period without any answer arriving.
    /// </summary>
    GracePeriodExpired,

    /// <summary>
    /// The session finished, its result has been fetched at least once, and no session activity happened within
    /// the idle timeout.
    /// </summary>
    IdleAfterCompletion,

    /// <summary>
    /// No session activity happened at all within the idle timeout.
    /// </summary>
    NoActivity
}
