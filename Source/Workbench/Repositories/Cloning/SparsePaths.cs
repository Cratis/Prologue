// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Works out which paths of a repository are checked out.
/// </summary>
public static class SparsePaths
{
    /// <summary>
    /// Normalizes a sub path: forward slashes, no leading or trailing slash.
    /// </summary>
    /// <param name="subPath">The sub path.</param>
    /// <returns>The normalized path; empty for the whole repository.</returns>
    public static string Normalize(SubPath subPath) => subPath.Value.Replace('\\', '/').Trim().Trim('/');

    /// <summary>
    /// Gets whether a sub path leaves the repository.
    /// </summary>
    /// <param name="subPath">The sub path.</param>
    /// <returns>True when it does.</returns>
    public static bool EscapesRepository(SubPath subPath) =>
        Normalize(subPath).Split('/').Any(segment => segment == ".." || segment == ".") || Path.IsPathRooted(Normalize(subPath));

    /// <summary>
    /// Gets the paths to check out so every system sharing a repository has its code. When any of them is the whole
    /// repository, nothing is narrowed.
    /// </summary>
    /// <param name="subPaths">Where each system lives.</param>
    /// <returns>The paths to check out; empty when the whole repository is.</returns>
    public static IReadOnlyList<string> For(IEnumerable<SubPath> subPaths)
    {
        var paths = subPaths.Select(Normalize).ToList();
        return paths.Exists(path => path.Length == 0) ? [] : [.. paths.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
    }
}
