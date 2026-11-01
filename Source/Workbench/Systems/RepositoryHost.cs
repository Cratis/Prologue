// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// The kind of place a system's code is hosted.
/// </summary>
public enum RepositoryHost
{
    /// <summary>
    /// GitHub, or a GitHub Enterprise server.
    /// </summary>
    GitHub,

    /// <summary>
    /// Azure DevOps.
    /// </summary>
    AzureDevOps,

    /// <summary>
    /// GitLab, or a self-hosted GitLab.
    /// </summary>
    GitLab,

    /// <summary>
    /// A folder on the Receiver's file system.
    /// </summary>
    LocalFolder
}
