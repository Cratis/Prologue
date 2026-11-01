// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Discover.Running;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discover.Map;

/// <summary>
/// Where the analysis of one system or folder stands: what it reads, how far it has come and how the last one ended. Progress
/// such as files read is held in memory and never recorded as events.
/// </summary>
/// <param name="Subject">The system or folder.</param>
/// <param name="Name">What it is called.</param>
/// <param name="State">Where the analysis stands.</param>
/// <param name="FilesRead">The number of files read so far.</param>
/// <param name="TotalFiles">The number of files that will be read.</param>
/// <param name="Host">Where the code is hosted.</param>
/// <param name="Repository">The repository, or the folder.</param>
/// <param name="Branch">The branch.</param>
/// <param name="SubPath">The path inside the repository.</param>
/// <param name="Commit">The commit or folder last read completely, when there is one.</param>
/// <param name="Reason">Why the last analysis could not finish, when it could not.</param>
/// <param name="Files">The files being read, in order.</param>
[ReadModel]
public record AnalysisStatus(
    AnalysisSubjectId Subject,
    SubjectName Name,
    AnalysisState State,
    FileCount FilesRead,
    FileCount TotalFiles,
    RepositoryHost Host,
    RepositoryName Repository,
    BranchName Branch,
    SubPath SubPath,
    AnalysisSource? Commit,
    FailureReason? Reason,
    IEnumerable<SourcePath> Files)
{
    /// <summary>
    /// Observes the analysis status of every system of a prologue and every folder analyzed for it.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="readModels">Observes the stored findings and the prologue.</param>
    /// <param name="progress">Holds the progress of running analyses.</param>
    /// <returns>The statuses.</returns>
    public static ISubject<IEnumerable<AnalysisStatus>> ObserveAnalysisStatuses(PrologueId prologue, IReadModels readModels, IAnalysisProgress progress) =>
        new DerivedSubject<IEnumerable<AnalysisStatus>>(
            ApplicationMap.Findings(prologue, readModels).CombineLatest(
                ApplicationMap.Definition(prologue, readModels),
                progress.Observe(),
                (findings, definition, running) => AnalysisStatuses.Build(prologue, findings, definition, running)));
}
