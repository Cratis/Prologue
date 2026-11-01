// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// Where a system's code lives.
/// </summary>
/// <param name="Host">The kind of host.</param>
/// <param name="Repository">The repository, or the folder for a local system.</param>
/// <param name="Branch">The branch.</param>
/// <param name="SubPath">The path inside the repository where the system lives; empty for the whole repository.</param>
public record SystemCodeLocation(RepositoryHost Host, RepositoryName Repository, BranchName Branch, SubPath SubPath);
