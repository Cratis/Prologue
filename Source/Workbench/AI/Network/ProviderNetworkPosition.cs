// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.Projections.ModelBound;
using Cratis.Prologue.Workbench.AI.Network.MarkingProviders;
using MongoDB.Driver;

namespace Cratis.Prologue.Workbench.AI.Network;

/// <summary>
/// Whether a person said a Cratis.AI provider runs inside their network. Cratis.AI has no such notion, so the Workbench
/// keeps the fact itself, keyed by the provider's id. A provider nobody marked has no entry.
/// </summary>
/// <param name="Id">The provider's id.</param>
/// <param name="IsInside">Whether the provider runs inside the network.</param>
[ReadModel]
public record ProviderNetworkPosition(
    string Id,
    [SetValue<ProviderMarkedAsInsideNetwork>(true)]
    [SetValue<ProviderMarkedAsOutsideNetwork>(false)]
    bool IsInside)
{
    /// <summary>
    /// Observes every provider that has been marked.
    /// </summary>
    /// <param name="collection">The projected positions.</param>
    /// <returns>The positions.</returns>
    public static ISubject<IEnumerable<ProviderNetworkPosition>> AllProviderNetworkPositions(IMongoCollection<ProviderNetworkPosition> collection) =>
        collection.Observe();
}
