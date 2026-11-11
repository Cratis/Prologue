// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discover.Analyzing;

/// <summary>
/// Works out what an analysis reads, or why it cannot start.
/// </summary>
public static class AnalysisTargets
{
    const string OutsideAllowedRoots = "That folder is outside the folders this Receiver may analyze (Prologue:Discover:AllowedRoots)";

    /// <summary>
    /// Resolves the code of a system. A system in a repository is read from its clone, so the repository has to be cloned
    /// first; a system in a local folder is read where it is.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="id">The system.</param>
    /// <param name="clone">The clone of the system's repository, when there is one.</param>
    /// <param name="cloneRoot">The directory repositories are cloned into.</param>
    /// <param name="isAllowed">Whether a folder on the Receiver may be analyzed; null allows every folder.</param>
    /// <returns>What to read, or a refusal.</returns>
    public static Result<AnalysisTarget, ValidationResult> ForSystem(PrologueDefinition? prologue, SystemId id, SystemClone? clone, string cloneRoot, Func<string, bool>? isAllowed = null)
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

        if (SparsePaths.EscapesRepository(location.SubPath))
        {
            return ValidationResult.Error("The system's sub path has to stay inside its repository");
        }

        var subPath = SparsePaths.Normalize(location.SubPath);
        var target = new AnalysisTarget(AnalysisSubjectId.For(id), new SubjectName(system.Name.Value), string.Empty, string.Empty, location.Host, location.Repository, location.Branch, location.SubPath);
        return location.Host == RepositoryHost.LocalFolder
            ? LocalTarget(target, subPath, clone, isAllowed)
            : ClonedTarget(target, prologue!, subPath, clone, cloneRoot);
    }

    /// <summary>
    /// Resolves a folder somebody pointed at.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <param name="folder">The folder.</param>
    /// <param name="exists">Whether the folder exists on the Receiver.</param>
    /// <param name="allowed">Whether the folder is one this Receiver may analyze.</param>
    /// <returns>What to read, or a refusal.</returns>
    public static Result<AnalysisTarget, ValidationResult> ForFolder(PrologueDefinition? prologue, FolderPath folder, bool exists, bool allowed = true)
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

        if (!allowed)
        {
            return ValidationResult.Error(OutsideAllowedRoots);
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

    static Result<AnalysisTarget, ValidationResult> LocalTarget(AnalysisTarget target, string subPath, SystemClone? clone, Func<string, bool>? isAllowed)
    {
        var directory = Path.Combine(target.Repository.Value.Trim(), subPath);
        if (isAllowed is not null && !isAllowed(directory))
        {
            return ValidationResult.Error(OutsideAllowedRoots);
        }

        var source = clone is { Status: CloneStatus.Cloned, Commit: { } head } ? head.Value : "working tree";
        return target with { Directory = directory, Source = source };
    }

    static Result<AnalysisTarget, ValidationResult> ClonedTarget(AnalysisTarget target, PrologueDefinition prologue, string subPath, SystemClone? clone, string cloneRoot)
    {
        if (clone is not { Status: CloneStatus.Cloned, Commit: { } commit })
        {
            return ValidationResult.Error("Clone the system's repository before analyzing it");
        }

        var request = ClonePlan.For(prologue, new SystemCodeLocation(target.Host, target.Repository, target.Branch, target.SubPath), null);
        return target with { Directory = Path.Combine(CloneDirectories.For(cloneRoot, request), subPath), Source = commit.Value };
    }
}
