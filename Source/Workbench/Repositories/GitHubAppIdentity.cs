// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common;

namespace Cratis.Prologue.Workbench.Repositories;

/// <summary>
/// What a GitHub App signs in with. Only the name of the secret holding the private key is kept, never the key.
/// </summary>
/// <param name="App">The app.</param>
/// <param name="PrivateKey">The name of the secret holding the app's private key in PEM form.</param>
/// <param name="Api">The address of the GitHub API, which differs from github.com's for GitHub Enterprise.</param>
public record GitHubAppIdentity(AppId App, SecretName PrivateKey, ServerUrl Api)
{
    /// <summary>
    /// The address of the GitHub API on github.com.
    /// </summary>
    public static readonly ServerUrl GitHubApi = new("https://api.github.com");
}
