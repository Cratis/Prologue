// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Running;

/// <summary>
/// Takes analyses to run in the background, one at a time.
/// </summary>
public interface IAnalysisQueue
{
    /// <summary>
    /// Queues an analysis. One that is already waiting or running for the same system or folder is not queued twice.
    /// </summary>
    /// <param name="job">The job.</param>
    /// <returns>True when it was queued; false when the system or folder is already being analyzed.</returns>
    bool Enqueue(AnalysisJob job);

    /// <summary>
    /// Waits for the next job.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The job.</returns>
    ValueTask<AnalysisJob> Next(CancellationToken cancellationToken);

    /// <summary>
    /// Says a job is done, so the same system or folder can be queued again.
    /// </summary>
    /// <param name="job">The job.</param>
    void Done(AnalysisJob job);
}
