// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Providers;
using Cratis.Arc.Chronicle.ReadModels;

namespace Cratis.Prologue.Workbench.AI.Network.MarkingProviders;

/// <summary>
/// Command for saying a provider runs inside the network.
/// </summary>
/// <param name="Provider">The <see cref="AIProviderId"/>.</param>
[Command]
public record MarkProviderAsInsideNetwork(AIProviderId Provider) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Provider.Value.ToString();

    /// <summary>
    /// Handles the command. Marking a provider the way it is already marked records nothing.
    /// </summary>
    /// <param name="current">The provider's position as it stands, if it was ever marked.</param>
    /// <returns>The event, or nothing when the provider is already inside.</returns>
    public IEnumerable<ProviderMarkedAsInsideNetwork> Handle([Unprotected("Marking again after a concurrent mark is harmless; the guard only avoids repeating the same fact")] ProviderNetworkPosition? current) =>
        current?.IsInside == true ? [] : [new ProviderMarkedAsInsideNetwork()];
}

/// <summary>
/// Command for saying a provider runs outside the network.
/// </summary>
/// <param name="Provider">The <see cref="AIProviderId"/>.</param>
[Command]
public record MarkProviderAsOutsideNetwork(AIProviderId Provider) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => Provider.Value.ToString();

    /// <summary>
    /// Handles the command. Marking a provider the way it is already marked records nothing.
    /// </summary>
    /// <param name="current">The provider's position as it stands, if it was ever marked.</param>
    /// <returns>The event, or nothing when the provider is already outside.</returns>
    public IEnumerable<ProviderMarkedAsOutsideNetwork> Handle([Unprotected("Marking again after a concurrent mark is harmless; the guard only avoids repeating the same fact")] ProviderNetworkPosition? current) =>
        current?.IsInside == false ? [] : [new ProviderMarkedAsOutsideNetwork()];
}

/// <summary>
/// Event raised when a provider was marked as running inside the network.
/// </summary>
[EventType]
public record ProviderMarkedAsInsideNetwork;

/// <summary>
/// Event raised when a provider was marked as running outside the network.
/// </summary>
[EventType]
public record ProviderMarkedAsOutsideNetwork;
