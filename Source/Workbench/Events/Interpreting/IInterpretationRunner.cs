// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Runs an interpretation: reads the evidence, models it, refines it, and records what differs from the last interpretation.
/// </summary>
public interface IInterpretationRunner
{
    /// <summary>
    /// Runs a job. A failure is reported through the progress of the prologue, never thrown.
    /// </summary>
    /// <param name="job">The job.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Run(InterpretationJob job, CancellationToken cancellationToken);
}
