// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Providers;
using Cratis.AI.Providers.Pools;
using Cratis.Prologue.Workbench.AI.Pools.Arranging;

namespace Cratis.Prologue.Workbench.AI.Pools.for_ArrangePool;

public class when_arranging_a_pool : Specification
{
    readonly CommandScenario<ArrangePool> _scenario = new();
    readonly AIProviderPoolId _pool = new(Guid.NewGuid());
    readonly AIProviderId _first = new(Guid.NewGuid());
    readonly AIProviderId _second = new(Guid.NewGuid());
    CommandResult _result;

    async Task Because() => _result = await _scenario.Execute(new ArrangePool(_pool, ModelTier.Powerful, [_first, _second, _first], PoolOrdering.InPriorityOrder, PoolFallback.PauseAndAsk));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] async Task should_record_the_arrangement() => await _scenario.EventSequence.ShouldHaveAppendedEvent<PoolArranged>(_ => _.Tier == ModelTier.Powerful && _.Ordering == PoolOrdering.InPriorityOrder && _.Fallback == PoolFallback.PauseAndAsk);
    [Fact] async Task should_keep_each_member_once() => await _scenario.EventSequence.ShouldHaveAppendedEvent<PoolArranged>(_ => _.Order.SequenceEqual([_first, _second]));
}
#endif
