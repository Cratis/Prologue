// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Chronicle.ReadModels;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Discover.Running;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Repositories;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Discover.Analyzing;

/// <summary>
/// Starts the analysis of a system's code. The request returns at once: the analysis runs in the background and its progress
/// is shown by <see cref="Map.AnalysisStatus"/>.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Id">The system.</param>
[Command]
public record AnalyzeSystemCode(PrologueId Prologue, SystemId Id)
{
    /// <summary>
    /// Works out what to read from the system's code location and its clone.
    /// </summary>
    /// <param name="readModels">Reads the prologue and the clone.</param>
    /// <param name="options">The configured clone directory.</param>
    /// <returns>What to read, or a refusal.</returns>
    [Unprotected("Analyzing only reads: a stale prologue or clone costs a repeated analysis, never a wrong decision")]
    public async Task<Result<AnalysisTarget, ValidationResult>> Provide(IReadModels readModels, IOptions<RepositoryOptions> options)
    {
        var prologue = await readModels.GetInstanceById<PrologueDefinition>((EventSourceId)Prologue);
        var clone = await readModels.GetInstanceById<SystemClone>((EventSourceId)SystemCloneId.For(Id));
        return AnalysisTargets.ForSystem(prologue, Id, clone, options.Value.CloneDirectory);
    }

    /// <summary>
    /// Handles the command by queueing the analysis.
    /// </summary>
    /// <param name="target">What to read, resolved by <see cref="Provide"/>.</param>
    /// <param name="queue">The queue of analyses.</param>
    public void Handle(AnalysisTarget target, IAnalysisQueue queue) => queue.Enqueue(new AnalysisJob(Prologue, target));
}
