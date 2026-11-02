// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// Mints the short-lived access tokens a GitHub App installation clones with.
/// </summary>
public interface IInstallationTokens
{
    /// <summary>
    /// Gets a token for an installation: one minted earlier that is not about to expire, or a new one.
    /// </summary>
    /// <param name="app">What the app signs in with.</param>
    /// <param name="installation">The installation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The token, or why none could be minted.</returns>
    Task<InstallationTokenResult> Mint(GitHubAppIdentity app, InstallationId installation, CancellationToken cancellationToken);
}
