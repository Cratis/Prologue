// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// What is needed to clone the repository of one system.
/// </summary>
/// <param name="Host">The kind of host.</param>
/// <param name="Repository">The repository, or the folder for a local system.</param>
/// <param name="Branch">The branch.</param>
/// <param name="SubPath">Where the system lives inside the repository; empty for the whole repository.</param>
/// <param name="SharedSubPaths">Where every system that shares this repository and branch lives, so one clone serves them all.</param>
/// <param name="Server">The server or organization address from the connection; empty to use the host's public default.</param>
/// <param name="Secret">The name of the secret holding the access token, when the connection uses one.</param>
/// <param name="Installation">The GitHub App installation, when the connection uses one.</param>
public record CloneRequest(
    RepositoryHost Host,
    RepositoryName Repository,
    BranchName Branch,
    SubPath SubPath,
    IEnumerable<SubPath> SharedSubPaths,
    ServerUrl? Server,
    SecretName? Secret,
    InstallationId? Installation);
