// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Clones the repository of a system read-only, so its code can be analyzed.
/// </summary>
public interface IRepositoryCloner
{
    /// <summary>
    /// Clones a repository, or brings an existing clone up to date. Never throws for a clone that fails: the failure is the result.
    /// </summary>
    /// <param name="request">What to clone.</param>
    /// <param name="cancellationToken">Cancels the clone.</param>
    /// <returns>Where the code is and at which commit, or why it could not be cloned.</returns>
    Task<CloneResult> Clone(CloneRequest request, CancellationToken cancellationToken);
}
