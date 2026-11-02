// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Pools.Spending;

namespace Cratis.Prologue.Workbench.AI.Pools.for_PoolSpend;

public class when_reducing_spent_tokens : Specification
{
    PoolSpend _first;
    PoolSpend _sameDay;
    PoolSpend _nextDay;

    void Because()
    {
        var context = EventContext.Empty with { EventSourceId = "pool-1" };
        _first = PoolSpend.Reduce(new PoolTokensSpent("2026-11-02", 100), null, context);
        _sameDay = PoolSpend.Reduce(new PoolTokensSpent("2026-11-02", 50), _first, context);
        _nextDay = PoolSpend.Reduce(new PoolTokensSpent("2026-11-03", 7), _sameDay, context);
    }

    [Fact] void should_key_the_spend_by_the_pool() => _first.Id.ShouldEqual("pool-1");
    [Fact] void should_add_to_the_same_day() => _sameDay.Tokens.Value.ShouldEqual(150);
    [Fact] void should_start_over_on_a_new_day() => _nextDay.Tokens.Value.ShouldEqual(7);
    [Fact] void should_count_nothing_for_a_day_other_than_the_one_spent_on() => _nextDay.On("2026-11-02").ShouldEqual(0);
    [Fact] void should_count_the_day_spent_on() => _sameDay.On("2026-11-02").ShouldEqual(150);
}
#endif
