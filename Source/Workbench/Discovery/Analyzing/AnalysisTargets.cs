// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discovery.Analyzing;

/// <summary>
/// Works out what an analysis reads, or why it cannot start.
/// </summary>
public static class AnalysisTargets
{
    /// <summary>
    /// Resolves the code of a system. A system in a repository is read from its clone, so the repository has to be cloned
    /// first; a system in a local folder is read where it is.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="id">The system.</param>
    /// <param name="clone">The clone of the system's repository, when there is one.</param>
    /// <param name="cloneRoot">The directory repositories are cloned into.</param>
    /// <returns>What to read, or a refusal.</returns>
    public static Result<AnalysisTarget, ValidationResult> ForSystem(PrologueDefinition? prologue, SystemId id, SystemClone? clone, string cloneRoot)
    {
        if (SystemRules.RefuseUnknown(prologue, id) is { } unknown)
        {
            return unknown;
        }

        var system = SystemRules.Find(prologue, id)!;
        if (system.Location is not { } location || string.IsNullOrWhiteSpace(location.Repository.Value))
        {
            return ValidationResult.Error("Say where the system's code lives before analyzing it");
        }

        var subject = AnalysisSubjectId.For(id);
        var name = new SubjectName(system.Name.Value);
        if (SparsePaths.EscapesRepository(location.SubPath))
        {
            return ValidationResult.Error("The system's sub path has to stay inside its repository");
        }

        var subPath = SparsePaths.Normalize(location.SubPath);

        if (location.Host == RepositoryHost.LocalFolder)
        {
            var directory = Path.Combine(location.Repository.Value.Trim(), subPath);
            var source = clone is { Status: CloneStatus.Cloned, Commit: { } head } ? head.Value : "working tree";
            return new AnalysisTarget(subject, name, directory, source, location.Host, location.Repository, location.Branch, location.SubPath);
        }

        if (clone is not { Status: CloneStatus.Cloned, Commit: { } commit })
        {
            return ValidationResult.Error("Clone the system's repository before analyzing it");
        }

        var request = ClonePlan.For(prologue!, location, null);
        return new AnalysisTarget(
            subject,
            name,
            Path.Combine(CloneDirectories.For(cloneRoot, request), subPath),
            commit.Value,
            location.Host,
            location.Repository,
            location.Branch,
            location.SubPath);
    }

    /// <summary>
    /// Resolves a folder somebody pointed at.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="folder">The folder.</param>
    /// <param name="exists">Whether the folder exists on the Receiver.</param>
    /// <returns>What to read, or a refusal.</returns>
    public static Result<AnalysisTarget, ValidationResult> ForFolder(PrologueDefinition? prologue, FolderPath folder, bool exists)
    {
        if (prologue is null)
        {
            return ValidationResult.Error("The prologue is not defined");
        }

        var path = folder.Value.Trim();
        if (path.Length == 0)
        {
            return ValidationResult.Error("Type a folder path first");
        }

        if (!exists)
        {
            return ValidationResult.Error($"The folder '{path}' does not exist on the Receiver");
        }

        var name = Path.GetFileName(path.TrimEnd('/', '\\'));
        return new AnalysisTarget(
            AnalysisSubjectId.ForFolder(path),
            new SubjectName(name.Length == 0 ? path : name),
            path,
            path,
            RepositoryHost.LocalFolder,
            new RepositoryName(path),
            new BranchName(string.Empty),
            new SubPath(string.Empty));
    }
}
