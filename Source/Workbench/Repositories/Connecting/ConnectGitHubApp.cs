// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Connecting;

/// <summary>
/// Connects GitHub through the installation of a GitHub App. The Receiver signs in as the app with its private key, held as a
/// secret and referenced by name, and mints a short-lived token for the installation whenever it clones.
/// </summary>
/// <param name="Url">The GitHub server.</param>
/// <param name="Installation">The installation.</param>
/// <param name="App">The app.</param>
/// <param name="PrivateKey">The name of the secret holding the app's private key.</param>
/// <param name="Api">The address of the GitHub API; empty for github.com, whose API is at https://api.github.com.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record ConnectGitHubApp(ServerUrl Url, InstallationId Installation, AppId App, SecretName PrivateKey, ServerUrl Api) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => RepositoryConnectionId.For(RepositoryHost.GitHub);

    /// <summary>
    /// Handles the command, refusing an address that is not a server, an API that is not https on the server, a private key that
    /// is not a secret name and a connection that is already recorded.
    /// </summary>
    /// <param name="current">The current connection.</param>
    /// <returns>The connection, or a refusal.</returns>
    public Result<GitHubAppInstallationConnected, ValidationResult> Handle(RepositoryConnection? current)
    {
        var url = new ServerUrl(Url.Value.Trim());
        var api = new ServerUrl(string.IsNullOrWhiteSpace(Api.Value) ? GitHubAppIdentity.GitHubApi.Value : Api.Value.Trim().TrimEnd('/'));
        var refused = ConnectionRules.RefuseUrl(url) ?? ConnectionRules.RefuseApp(url, api, App, Installation) ?? ConnectionRules.RefuseSecretName(PrivateKey);
        if (refused is { } refusal)
        {
            return refusal;
        }

        var connected = new GitHubAppInstallationConnected(url, Installation, new GitHubAppIdentity(App, new SecretName(PrivateKey.Value.Trim()), api));
        return current is { Host: RepositoryHost.GitHub } && current.Url == connected.Url && current.Installation == Installation && current.App == connected.App
            ? ValidationResult.Error("GitHub is already connected like this")
            : connected;
    }
}

/// <summary>
/// GitHub was connected through the installation of a GitHub App, which the Receiver signs in as with a private key it resolves by name.
/// </summary>
/// <param name="Url">The GitHub server.</param>
/// <param name="Installation">The installation.</param>
/// <param name="App">What the app signs in with.</param>
[EventType]
public record GitHubAppInstallationConnected(ServerUrl Url, InstallationId Installation, GitHubAppIdentity App);
