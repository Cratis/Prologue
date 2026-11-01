// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Connecting;

/// <summary>
/// Connects an Azure DevOps organization with an access token held in a secret.
/// </summary>
/// <param name="Organization">The organization address, such as <c language="text">https://dev.azure.com/contoso</c>.</param>
/// <param name="Secret">The name of the secret holding the access token.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record ConnectAzureDevOps(ServerUrl Organization, SecretName Secret) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => RepositoryConnectionId.For(RepositoryHost.AzureDevOps);

    /// <summary>
    /// Handles the command, refusing an address that is not a server, a token that is not a secret name, and a connection that is already recorded.
    /// </summary>
    /// <param name="current">The current connection.</param>
    /// <returns>The connection, or a refusal.</returns>
    public Result<AzureDevOpsConnected, ValidationResult> Handle(RepositoryConnection? current)
    {
        if (ConnectionRules.Refuse(Organization, Secret) is { } refusal)
        {
            return refusal;
        }

        var connected = new AzureDevOpsConnected(new ServerUrl(Organization.Value.Trim().TrimEnd('/')), new SecretName(Secret.Value.Trim()));
        return current is { Host: RepositoryHost.AzureDevOps } && current.Url == connected.Organization && current.Secret == connected.Secret
            ? ValidationResult.Error("Azure DevOps is already connected like this")
            : connected;
    }
}

/// <summary>
/// An Azure DevOps organization was connected.
/// </summary>
/// <param name="Organization">The organization address.</param>
/// <param name="Secret">The name of the secret holding the access token.</param>
[EventType]
public record AzureDevOpsConnected(ServerUrl Organization, SecretName Secret);
