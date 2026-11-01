// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Chronicle.ReadModels;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Clones the repository of a system read-only, or brings its clone up to date. Systems in the same repository share one
/// clone. A local folder is used where it is.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Id">The system.</param>
[Command]
public record CloneSystemRepository(PrologueId Prologue, SystemId Id) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => SystemCloneId.For(Id);

    /// <summary>
    /// Works out what to clone from the system's code location and its host's connection. A host with no connection is
    /// cloned from anonymously, which only works for public repositories.
    /// </summary>
    /// <param name="readModels">Reads the prologue, the connection and the clone.</param>
    /// <returns>What to clone, or a refusal.</returns>
    [Unprotected("Cloning only reads: a stale prologue, connection or clone state costs a retry or one repeated event, never a wrong decision")]
    public async Task<Result<CloneTarget, ValidationResult>> Provide(IReadModels readModels)
    {
        var prologue = await readModels.GetInstanceById<PrologueDefinition>((EventSourceId)Prologue);
        if (SystemRules.RefuseUnknown(prologue, Id) is { } unknown)
        {
            return unknown;
        }

        if (SystemRules.Find(prologue, Id)!.Location is not { } location)
        {
            return ValidationResult.Error("Say where the system's code lives before cloning it");
        }

        var connection = location.Host == RepositoryHost.LocalFolder
            ? null
            : await readModels.GetInstanceById<RepositoryConnection>((EventSourceId)RepositoryConnectionId.For(location.Host));
        var previous = await readModels.GetInstanceById<SystemClone>((EventSourceId)SystemCloneId.For(Id));

        return new CloneTarget(ClonePlan.For(prologue, location, connection), previous);
    }

    /// <summary>
    /// Handles the command by cloning and recording a change in how the clone stands.
    /// </summary>
    /// <param name="target">What to clone, resolved by <see cref="Provide"/>.</param>
    /// <param name="cloner">Clones the repository.</param>
    /// <returns>The clone or the failure when it differs from the last one; nothing when it does not.</returns>
    public async Task<IEnumerable<object>> Handle(CloneTarget target, IRepositoryCloner cloner) =>
        CloneDecision.Decide(await cloner.Clone(target.Request, CancellationToken.None), target.Previous);
}

/// <summary>
/// The repository of a system was cloned, or its clone was brought to a new commit.
/// </summary>
/// <param name="Commit">The commit the clone is at.</param>
/// <param name="Files">The number of files under the system's sub path.</param>
[EventType]
public record SystemRepositoryCloned(CommitId Commit, FileCount Files);

/// <summary>
/// Cloning the repository of a system failed.
/// </summary>
/// <param name="Reason">Why it failed, with credentials redacted.</param>
[EventType]
public record SystemRepositoryCloneFailed(FailureReason Reason);
