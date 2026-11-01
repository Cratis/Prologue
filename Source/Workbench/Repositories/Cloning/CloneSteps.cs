// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// The calls to git that clone a repository, or bring a clone up to date. The history is always shallow, only the branch
/// is fetched and, when systems live in sub paths, only those paths are checked out.
/// </summary>
public static class CloneSteps
{
    /// <summary>
    /// Gets the steps that make a new clone.
    /// </summary>
    /// <param name="url">The address to clone from, without credentials.</param>
    /// <param name="directory">The directory to clone into.</param>
    /// <param name="branch">The branch.</param>
    /// <param name="sparse">The paths to check out; empty for the whole repository.</param>
    /// <returns>The steps.</returns>
    public static IReadOnlyList<CloneStep> Clone(string url, string directory, string branch, IReadOnlyList<string> sparse)
    {
        List<string> clone = ["clone", "--depth=1", "--single-branch", $"--branch={branch}"];
        if (sparse.Count > 0)
        {
            clone.AddRange(["--filter=blob:none", "--sparse"]);
        }

        clone.AddRange(["--", url, directory]);
        return [new CloneStep(clone, false), .. Sparse(sparse)];
    }

    /// <summary>
    /// Gets the steps that bring an existing clone up to date, including paths other systems have added.
    /// </summary>
    /// <param name="branch">The branch.</param>
    /// <param name="sparse">The paths to check out; empty for the whole repository.</param>
    /// <returns>The steps.</returns>
    public static IReadOnlyList<CloneStep> Update(string branch, IReadOnlyList<string> sparse) =>
    [
        new CloneStep(["fetch", "--depth=1", "origin", $"+{branch}"], true),
        new CloneStep(["checkout", "--force", "--detach", "FETCH_HEAD"], true),
        .. Sparse(sparse)
    ];

    static IEnumerable<CloneStep> Sparse(IReadOnlyList<string> sparse) =>
        sparse.Count == 0
            ? []
            : [new CloneStep(["sparse-checkout", "set", "--cone", "--", .. sparse], true)];
}
