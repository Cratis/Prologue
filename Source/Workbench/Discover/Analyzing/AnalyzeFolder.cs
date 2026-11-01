// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Chronicle.ReadModels;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Discover.Running;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Analyzing;

/// <summary>
/// Starts the analysis of any folder on the machine the Receiver runs on, such as a legacy codebase that is not one of the
/// prologue's systems. The same folder is the same subject, so analyzing it again updates its findings.
/// </summary>
/// <param name="Prologue">The prologue the findings belong to.</param>
/// <param name="Folder">The folder.</param>
[Command]
public record AnalyzeFolder(PrologueId Prologue, FolderPath Folder)
{
    /// <summary>
    /// Checks the prologue and the folder.
    /// </summary>
    /// <param name="readModels">Reads the prologue.</param>
    /// <param name="folders">Looks at the file system.</param>
    /// <returns>What to read, or a refusal.</returns>
    [Unprotected("Analyzing only reads: a stale prologue costs a repeated analysis, never a wrong decision")]
    public async Task<Result<AnalysisTarget, ValidationResult>> Provide(IReadModels readModels, ISourceFolders folders)
    {
        var prologue = await readModels.GetInstanceById<PrologueDefinition>((EventSourceId)Prologue);
        return AnalysisTargets.ForFolder(prologue, Folder, folders.Exists(Folder.Value));
    }

    /// <summary>
    /// Handles the command by queueing the analysis.
    /// </summary>
    /// <param name="target">What to read, resolved by <see cref="Provide"/>.</param>
    /// <param name="queue">The queue of analyses.</param>
    public void Handle(AnalysisTarget target, IAnalysisQueue queue) => queue.Enqueue(new AnalysisJob(Prologue, target));
}
