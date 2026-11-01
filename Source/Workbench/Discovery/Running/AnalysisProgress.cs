// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discovery.Analyzing;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Repositories.Cloning;

namespace Cratis.Prologue.Workbench.Discovery.Running;

/// <summary>
/// How far one running analysis has come. It lives in memory: progress is not a fact about the domain, so it is never an event.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Target">What is being read.</param>
/// <param name="State">Where the analysis stands.</param>
/// <param name="FilesRead">The number of files read so far.</param>
/// <param name="Files">The files being read, in order, up to a limit.</param>
/// <param name="TotalFiles">The number of files that will be read.</param>
/// <param name="Reason">Why it failed, when it did.</param>
public record AnalysisProgress(PrologueId Prologue, AnalysisTarget Target, AnalysisState State, int FilesRead, IReadOnlyList<SourcePath> Files, int TotalFiles, FailureReason? Reason)
{
    /// <summary>
    /// Gets progress for an analysis that is waiting.
    /// </summary>
    /// <param name="job">The job.</param>
    /// <returns>The progress.</returns>
    public static AnalysisProgress Queued(AnalysisJob job) => new(job.Prologue, job.Target, AnalysisState.Queued, 0, [], 0, null);
}
