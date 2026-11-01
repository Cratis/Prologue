// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Providers;
using Cratis.AI.Providers.Pools;
using Cratis.Prologue.Workbench.AI.Pools.Arranging;

namespace Cratis.Prologue.Workbench.AI.Pools.for_ArrangePool;

public class when_arranging_a_pool_the_way_it_already_is : Specification
{
    readonly CommandScenario<ArrangePool> _scenario = new();
    readonly AIProviderPoolId _pool = new(Guid.NewGuid());
    readonly AIProviderId _provider = new(Guid.NewGuid());
    CommandResult _result;

    void Establish() => _scenario.Given.ForEventSource(_pool.Value.ToString()).ReadModel(new PoolArrangement(_pool.Value.ToString(), ModelTier.Fast, [_provider], PoolOrdering.ByRemainingCapacity, PoolFallback.ContinueOnNext));

    async Task Because() => _result = await _scenario.Execute(new ArrangePool(_pool, ModelTier.Fast, [_provider], PoolOrdering.ByRemainingCapacity, PoolFallback.ContinueOnNext));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] async Task should_append_nothing() => await _scenario.EventSequence.ShouldHaveTailSequenceNumber(EventSequenceNumber.Unavailable);
}
#endif
