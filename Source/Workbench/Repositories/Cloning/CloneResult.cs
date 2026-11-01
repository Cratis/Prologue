// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// What cloning the repository of a system ended in.
/// </summary>
/// <param name="Commit">The commit the clone is at, when it succeeded.</param>
/// <param name="Files">The number of files under the system's sub path, when it succeeded.</param>
/// <param name="Directory">Where the system's code is on this machine, when it succeeded.</param>
/// <param name="Failure">Why it failed, when it failed.</param>
public record CloneResult(CommitId? Commit, FileCount? Files, string? Directory, FailureReason? Failure)
{
    /// <summary>
    /// Gets a value indicating whether the clone succeeded.
    /// </summary>
    public bool Succeeded => Failure is null;

    /// <summary>
    /// Creates the result of a clone that succeeded.
    /// </summary>
    /// <param name="commit">The commit.</param>
    /// <param name="files">The files under the system's sub path.</param>
    /// <param name="directory">Where the system's code is.</param>
    /// <returns>The result.</returns>
    public static CloneResult Cloned(CommitId commit, FileCount files, string directory) => new(commit, files, directory, null);

    /// <summary>
    /// Creates the result of a clone that failed.
    /// </summary>
    /// <param name="reason">Why it failed.</param>
    /// <returns>The result.</returns>
    public static CloneResult Failed(string reason) => new(null, null, null, new FailureReason(reason));
}
