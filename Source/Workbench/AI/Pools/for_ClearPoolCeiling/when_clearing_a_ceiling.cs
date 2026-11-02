// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Providers.Pools;
using Cratis.Prologue.Workbench.AI.Pools.Ceiling;

namespace Cratis.Prologue.Workbench.AI.Pools.for_ClearPoolCeiling;

public class when_clearing_a_ceiling : Specification
{
    readonly CommandScenario<ClearPoolCeiling> _scenario = new();
    readonly CommandScenario<ClearPoolCeiling> _none = new();
    readonly AIProviderPoolId _pool = new(Guid.NewGuid());
    CommandResult _result;

    void Establish() => _scenario.Given.ForEventSource(_pool.Value.ToString()).ReadModel(new PoolCeiling(_pool.Value.ToString(), 500));

    async Task Because()
    {
        _result = await _scenario.Execute(new ClearPoolCeiling(_pool));
        await _none.Execute(new ClearPoolCeiling(_pool));
    }

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] async Task should_record_that_the_ceiling_was_cleared() => await _scenario.EventSequence.ShouldHaveAppendedEvent<PoolCeilingCleared>(_ => true);
    [Fact] async Task should_record_nothing_for_a_pool_without_a_ceiling() => await _none.EventSequence.ShouldHaveTailSequenceNumber(EventSequenceNumber.Unavailable);
}
#endif
