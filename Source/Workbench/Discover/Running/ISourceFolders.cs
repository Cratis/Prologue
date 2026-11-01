// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discover.Running;

/// <summary>
/// Looks at the folders on the machine the Receiver runs on.
/// </summary>
public interface ISourceFolders
{
    /// <summary>
    /// Gets whether a folder exists.
    /// </summary>
    /// <param name="path">The folder.</param>
    /// <returns>True when it does.</returns>
    bool Exists(string path);
}
