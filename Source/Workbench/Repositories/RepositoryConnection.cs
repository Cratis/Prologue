// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Common;
using Cratis.Prologue.Workbench.Repositories.Connecting;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories;

/// <summary>
/// The connection to a repository host. Only the name of the secret holding the access token is kept, never the token.
/// </summary>
/// <param name="Id">The connection, one per host.</param>
/// <param name="Host">The host.</param>
/// <param name="Url">The server or organization address.</param>
/// <param name="Secret">The name of the secret holding the access token, when the connection uses one.</param>
/// <param name="Installation">The GitHub App installation, when the connection uses one.</param>
[ReadModel]
public record RepositoryConnection(RepositoryConnectionId Id, RepositoryHost Host, ServerUrl Url, SecretName? Secret, InstallationId? Installation)
{
    /// <summary>
    /// Observes the connected hosts.
    /// </summary>
    /// <param name="readModels">Observes the stored connections.</param>
    /// <returns>The connections.</returns>
    public static ISubject<IEnumerable<RepositoryConnection>> ObserveRepositoryConnections(IReadModels readModels) =>
        readModels.Observe<RepositoryConnection>();

    /// <summary>
    /// Reduces GitHub being connected with a token.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The new state.</returns>
    public static RepositoryConnection Reduce(GitHubConnected @event, RepositoryConnection? current, EventContext context) =>
        new(new RepositoryConnectionId(context.EventSourceId.Value), RepositoryHost.GitHub, @event.Url, @event.Secret, null);

    /// <summary>
    /// Reduces GitHub being connected through an app.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The new state.</returns>
    public static RepositoryConnection Reduce(GitHubAppConnected @event, RepositoryConnection? current, EventContext context) =>
        new(new RepositoryConnectionId(context.EventSourceId.Value), RepositoryHost.GitHub, @event.Url, null, @event.Installation);

    /// <summary>
    /// Reduces Azure DevOps being connected.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The new state.</returns>
    public static RepositoryConnection Reduce(AzureDevOpsConnected @event, RepositoryConnection? current, EventContext context) =>
        new(new RepositoryConnectionId(context.EventSourceId.Value), RepositoryHost.AzureDevOps, @event.Organization, @event.Secret, null);

    /// <summary>
    /// Reduces GitLab being connected.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The new state.</returns>
    public static RepositoryConnection Reduce(GitLabConnected @event, RepositoryConnection? current, EventContext context) =>
        new(new RepositoryConnectionId(context.EventSourceId.Value), RepositoryHost.GitLab, @event.Url, @event.Secret, null);

    /// <summary>
    /// Reduces a host being disconnected.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>No state: a disconnected host has no connection.</returns>
    public static RepositoryConnection? Reduce(RepositoryHostDisconnected @event, RepositoryConnection? current) => null;
}
