// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// The state of the clone of one system's repository: the commit it is at, when, how many files the system has and why a clone failed.
/// </summary>
/// <param name="Id">The system whose repository this is.</param>
/// <param name="Status">Where the clone stands.</param>
/// <param name="Commit">The commit last cloned, when one was.</param>
/// <param name="Files">The number of files under the system's sub path at that commit.</param>
/// <param name="Reason">Why the last attempt failed, when it did; credentials are redacted.</param>
/// <param name="Occurred">When the state was last recorded.</param>
[ReadModel]
public record SystemClone(SystemCloneId Id, CloneStatus Status, CommitId? Commit, FileCount? Files, FailureReason? Reason, DateTimeOffset Occurred)
{
    /// <summary>
    /// Observes the clones of every system.
    /// </summary>
    /// <param name="collection">The projected clones.</param>
    /// <returns>The clones.</returns>
    public static ISubject<IEnumerable<SystemClone>> ObserveSystemClones(IMongoCollection<SystemClone> collection) =>
        collection.Observe();

    /// <summary>
    /// Reduces a repository being cloned.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The new state.</returns>
    public static SystemClone Reduce(SystemRepositoryCloned @event, SystemClone? current, EventContext context) =>
        new(new SystemCloneId(Guid.Parse(context.EventSourceId.Value)), CloneStatus.Cloned, @event.Commit, @event.Files, null, context.Occurred);

    /// <summary>
    /// Reduces a clone failing.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The new state, which keeps what was last cloned.</returns>
    public static SystemClone Reduce(SystemRepositoryCloneFailed @event, SystemClone? current, EventContext context) =>
        new(new SystemCloneId(Guid.Parse(context.EventSourceId.Value)), CloneStatus.Failed, current?.Commit, current?.Files, @event.Reason, context.Occurred);
}
