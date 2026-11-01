// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discovery.Analyzing;

/// <summary>
/// What an analysis reads and where it came from.
/// </summary>
/// <param name="Subject">The system or folder.</param>
/// <param name="Name">What it is called.</param>
/// <param name="Directory">The directory on disk whose files are read.</param>
/// <param name="Source">The commit, or the folder, that is read.</param>
/// <param name="Host">Where the code is hosted; a local folder for an analyzed folder.</param>
/// <param name="Repository">The repository, or the folder.</param>
/// <param name="Branch">The branch; empty for a folder.</param>
/// <param name="SubPath">The path inside the repository; empty for a folder.</param>
public record AnalysisTarget(
    AnalysisSubjectId Subject,
    SubjectName Name,
    FolderPath Directory,
    AnalysisSource Source,
    RepositoryHost Host,
    RepositoryName Repository,
    BranchName Branch,
    SubPath SubPath);
