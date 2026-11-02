// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Providers.Pools;
using Cratis.Prologue.Workbench.AI.Pools.Ceiling;

namespace Cratis.Prologue.Workbench.AI.Pools.for_SetPoolCeiling;

public class when_setting_a_ceiling : Specification
{
    readonly CommandScenario<SetPoolCeiling> _scenario = new();
    readonly CommandScenario<SetPoolCeiling> _same = new();
    readonly AIProviderPoolId _pool = new(Guid.NewGuid());
    CommandResult _result;
    CommandResult _zero;
    CommandResult _repeated;

    void Establish() => _same.Given.ForEventSource(_pool.Value.ToString()).ReadModel(new PoolCeiling(_pool.Value.ToString(), 500));

    async Task Because()
    {
        _result = await _scenario.Execute(new SetPoolCeiling(_pool, 500));
        _zero = await _scenario.Execute(new SetPoolCeiling(_pool, 0));
        _repeated = await _same.Execute(new SetPoolCeiling(_pool, 500));
    }

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] async Task should_record_the_ceiling() => await _scenario.EventSequence.ShouldHaveAppendedEvent<PoolCeilingSet>(_ => _.TokensPerDay.Value == 500);
    [Fact] void should_refuse_zero_tokens() => _zero.IsValid.ShouldBeFalse();
    [Fact] async Task should_record_nothing_for_the_ceiling_the_pool_already_has() => await _same.EventSequence.ShouldHaveTailSequenceNumber(EventSequenceNumber.Unavailable);
}
#endif
