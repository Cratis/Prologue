// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Interprets a prologue's captures into suggested events. The work runs in the background: the command returns at once and the progress
/// is shown by <see cref="InterpretationStatus"/>. Interpreting what is being interpreted already does nothing.
/// </summary>
/// <param name="Prologue">The prologue.</param>
[Command]
public record RunInterpretation(PrologueId Prologue)
{
    /// <summary>
    /// Handles the command by queueing the interpretation.
    /// </summary>
    /// <param name="queue">The queue.</param>
    public void Handle(IInterpretationQueue queue) => queue.Enqueue(new InterpretationJob(Prologue, false));
}
