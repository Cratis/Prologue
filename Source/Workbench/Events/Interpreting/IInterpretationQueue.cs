// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Holds the interpretations that wait for their turn, so no request waits for the work.
/// </summary>
public interface IInterpretationQueue
{
    /// <summary>
    /// Queues an interpretation. One that is already waiting or running for the same prologue is not queued twice.
    /// </summary>
    /// <param name="job">The job.</param>
    /// <returns>True when it was queued; false when the prologue is being interpreted already.</returns>
    bool Enqueue(InterpretationJob job);

    /// <summary>
    /// Waits for the next job.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The job.</returns>
    ValueTask<InterpretationJob> Next(CancellationToken cancellationToken);

    /// <summary>
    /// Says a job is done, so the prologue can be interpreted again.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    void Done(PrologueId prologue);
}
