// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Connecting;

/// <summary>
/// Connects GitHub through the installation of a GitHub App.
/// </summary>
/// <param name="Url">The GitHub server.</param>
/// <param name="Installation">The installation.</param>
[Command]
public record ConnectGitHubApp(ServerUrl Url, InstallationId Installation) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => RepositoryConnectionId.For(RepositoryHost.GitHub);

    /// <summary>
    /// Handles the command, refusing an address that is not a server and a connection that is already recorded.
    /// </summary>
    /// <param name="current">The current connection.</param>
    /// <returns>The connection, or a refusal.</returns>
    public Result<GitHubAppConnected, ValidationResult> Handle(RepositoryConnection? current)
    {
        if (ConnectionRules.RefuseUrl(Url) is { } refusal)
        {
            return refusal;
        }

        var connected = new GitHubAppConnected(new ServerUrl(Url.Value.Trim()), Installation);
        return current is { Host: RepositoryHost.GitHub } && current.Url == connected.Url && current.Installation == Installation
            ? ValidationResult.Error("GitHub is already connected like this")
            : connected;
    }
}

/// <summary>
/// GitHub was connected through the installation of a GitHub App.
/// </summary>
/// <param name="Url">The GitHub server.</param>
/// <param name="Installation">The installation.</param>
[EventType]
public record GitHubAppConnected(ServerUrl Url, InstallationId Installation);
