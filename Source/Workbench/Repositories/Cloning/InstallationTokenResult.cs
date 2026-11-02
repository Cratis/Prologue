// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// The outcome of minting an installation access token. A token is a credential: it is kept in memory only.
/// </summary>
/// <param name="Token">The token, or null when none could be minted.</param>
/// <param name="Failure">Why none could be minted, redacted and safe to show; empty when there is a token.</param>
public record InstallationTokenResult(string? Token, string Failure)
{
    /// <summary>
    /// Gets a value indicating whether a token was minted.
    /// </summary>
    public bool Succeeded => Token is not null;

    /// <summary>
    /// A token that was minted.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The result.</returns>
    public static InstallationTokenResult Minted(string token) => new(token, string.Empty);

    /// <summary>
    /// No token could be minted.
    /// </summary>
    /// <param name="failure">Why.</param>
    /// <returns>The result.</returns>
    public static InstallationTokenResult Failed(string failure) => new(null, failure);
}
