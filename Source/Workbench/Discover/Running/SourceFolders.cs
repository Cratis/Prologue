// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Running;

/// <summary>
/// Looks at the file system.
/// </summary>
public class SourceFolders : ISourceFolders
{
    /// <inheritdoc/>
    public bool Exists(string path) => Directory.Exists(path.Trim());
}
