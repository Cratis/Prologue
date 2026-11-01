// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Workbench.Discover.Running;

/// <summary>
/// Looks at the folders of the machine the Receiver runs on, limited to the allowed roots when some are configured.
/// </summary>
/// <param name="options">The configured allowed roots.</param>
public class SourceFolders(IOptions<DiscoverOptions> options) : ISourceFolders
{
    /// <inheritdoc/>
    public bool Exists(string path) => Directory.Exists(path.Trim());

    /// <inheritdoc/>
    public bool IsAllowed(string path)
    {
        var roots = options.Value.AllowedRoots.Where(root => !string.IsNullOrWhiteSpace(root)).ToList();
        if (roots.Count == 0)
        {
            return true;
        }

        var target = Resolve(path);
        return roots.Exists(root => AllowedRoots.Contains(Resolve(root), target));
    }

    static string Resolve(string path)
    {
        // Full paths resolve "." and ".." segments, so a path cannot climb out of a root; a folder that is itself a link
        // counts as where it points.
        var full = Path.GetFullPath(path.Trim());
        try
        {
            var target = Directory.ResolveLinkTarget(full, returnFinalTarget: true);
            return target is null ? full : Path.GetFullPath(target.FullName);
        }
        catch (IOException)
        {
            return full;
        }
    }
}
