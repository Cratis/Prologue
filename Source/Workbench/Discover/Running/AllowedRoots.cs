// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Running;

/// <summary>
/// The rule for which folders are inside an allowed root.
/// </summary>
public static class AllowedRoots
{
    /// <summary>
    /// Gets whether a folder is a root or inside it. Both are full paths.
    /// </summary>
    /// <param name="root">The allowed root.</param>
    /// <param name="folder">The folder.</param>
    /// <returns>True when the folder is the root or below it.</returns>
    public static bool Contains(string root, string folder)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedFolder = folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.Equals(normalizedFolder, normalizedRoot, comparison)
            || normalizedFolder.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, comparison);
    }
}
