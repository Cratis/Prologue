// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents the outward-facing view of an interpreter session — what a consumer polling the service needs to
/// drive its user experience: the stage the session is at, the questions waiting for answers, the system name once
/// derived, and what went wrong when the session failed.
/// </summary>
/// <param name="Status">The stage the session is at.</param>
/// <param name="PendingQuestions">The questions the language model asked that have not been answered yet.</param>
/// <param name="SystemName">The system name derived for the captured system; empty until derived.</param>
/// <param name="Error">What went wrong; empty unless the session failed.</param>
public record SessionSnapshot(
    InterpreterStatus Status,
    IReadOnlyList<InterpreterQuestion> PendingQuestions,
    string SystemName,
    string Error)
{
    /// <summary>
    /// Maps a session state to its snapshot.
    /// </summary>
    /// <param name="state">The <see cref="InterpreterSessionState"/> to map.</param>
    /// <returns>The <see cref="SessionSnapshot"/> for the state.</returns>
    public static SessionSnapshot From(InterpreterSessionState state) =>
        new(state.Status, state.PendingQuestions, state.Model?.SystemName ?? string.Empty, state.Error);
}
