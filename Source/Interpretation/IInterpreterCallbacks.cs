// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Defines the host-side callbacks a synchronous interpreter run surfaces its interaction through — a CLI blocks
/// in <see cref="OnQuestion"/> to prompt the user, a batch host answers without interaction. One object carries
/// both concerns: the <see cref="InterpreterRunner"/> invokes <see cref="OnQuestion"/> for every pending question
/// one at a time, and the host wires <see cref="OnStatusChanged"/> into the session factory's status callback when
/// it creates the session the runner drives.
/// </summary>
public interface IInterpreterCallbacks
{
    /// <summary>
    /// Asks the host to answer one question — blocking until the answer is available. The user can always answer
    /// with free text on top of the question's choices.
    /// </summary>
    /// <param name="question">The question to answer.</param>
    /// <returns>The answer to the question.</returns>
    Task<InterpreterAnswer> OnQuestion(InterpreterQuestion question);

    /// <summary>
    /// Notifies the host that the session transitioned to a new status.
    /// </summary>
    /// <param name="status">The status the session transitioned to.</param>
    void OnStatusChanged(InterpreterStatus status);
}
