// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Where the clone of a system's repository stands.
/// </summary>
public enum CloneStatus
{
    /// <summary>
    /// The repository is cloned and its code is available.
    /// </summary>
    Cloned,

    /// <summary>
    /// The last attempt to clone failed.
    /// </summary>
    Failed
}
