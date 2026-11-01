// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Connecting;

/// <summary>
/// Disconnects a repository host. Systems on that host keep their code location but cannot be cloned until it is connected again.
/// </summary>
/// <param name="Host">The host.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record DisconnectRepositoryHost(RepositoryHost Host) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => RepositoryConnectionId.For(Host);

    /// <summary>
    /// Handles the command, refusing a host that is not connected.
    /// </summary>
    /// <param name="current">The current connection.</param>
    /// <returns>The disconnection, or a refusal.</returns>
    public Result<RepositoryHostDisconnected, ValidationResult> Handle(RepositoryConnection? current) =>
        current is null ? ValidationResult.Error("That host is not connected") : new RepositoryHostDisconnected(Host);
}

/// <summary>
/// A repository host was disconnected.
/// </summary>
/// <param name="Host">The host.</param>
[EventType]
public record RepositoryHostDisconnected(RepositoryHost Host);
