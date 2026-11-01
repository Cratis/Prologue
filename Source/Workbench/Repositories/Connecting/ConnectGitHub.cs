// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Connecting;

/// <summary>
/// Connects GitHub with an access token. The token is held in a secret and only the secret's name is recorded.
/// </summary>
/// <param name="Url">The GitHub server, <c language="text">https://github.com</c> for github.com.</param>
/// <param name="Secret">The name of the secret holding the access token.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record ConnectGitHub(ServerUrl Url, SecretName Secret) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => RepositoryConnectionId.For(RepositoryHost.GitHub);

    /// <summary>
    /// Handles the command, refusing an address that is not a server, a token that is not a secret name, and a connection that is already recorded.
    /// </summary>
    /// <param name="current">The current connection.</param>
    /// <returns>The connection, or a refusal.</returns>
    public Result<GitHubConnected, ValidationResult> Handle(RepositoryConnection? current)
    {
        if (ConnectionRules.Refuse(Url, Secret) is { } refusal)
        {
            return refusal;
        }

        var connected = new GitHubConnected(new ServerUrl(Url.Value.Trim()), new SecretName(Secret.Value.Trim()));
        return current is { Host: RepositoryHost.GitHub } && current.Url == connected.Url && current.Secret == connected.Secret
            ? ValidationResult.Error("GitHub is already connected like this")
            : connected;
    }
}

/// <summary>
/// GitHub was connected with an access token.
/// </summary>
/// <param name="Url">The GitHub server.</param>
/// <param name="Secret">The name of the secret holding the access token.</param>
[EventType]
public record GitHubConnected(ServerUrl Url, SecretName Secret);
