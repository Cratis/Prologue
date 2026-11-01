// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Connecting;

/// <summary>
/// Connects GitLab with an access token held in a secret.
/// </summary>
/// <param name="Url">The GitLab server, <c language="text">https://gitlab.com</c> for gitlab.com.</param>
/// <param name="Secret">The name of the secret holding the access token.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record ConnectGitLab(ServerUrl Url, SecretName Secret) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => RepositoryConnectionId.For(RepositoryHost.GitLab);

    /// <summary>
    /// Handles the command, refusing an address that is not a server, a token that is not a secret name, and a connection that is already recorded.
    /// </summary>
    /// <param name="current">The current connection.</param>
    /// <returns>The connection, or a refusal.</returns>
    public Result<GitLabConnected, ValidationResult> Handle(RepositoryConnection? current)
    {
        if (ConnectionRules.Refuse(Url, Secret) is { } refusal)
        {
            return refusal;
        }

        var connected = new GitLabConnected(new ServerUrl(Url.Value.Trim().TrimEnd('/')), new SecretName(Secret.Value.Trim()));
        return current is { Host: RepositoryHost.GitLab } && current.Url == connected.Url && current.Secret == connected.Secret
            ? ValidationResult.Error("GitLab is already connected like this")
            : connected;
    }
}

/// <summary>
/// GitLab was connected.
/// </summary>
/// <param name="Url">The GitLab server.</param>
/// <param name="Secret">The name of the secret holding the access token.</param>
[EventType]
public record GitLabConnected(ServerUrl Url, SecretName Secret);
