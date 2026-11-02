// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Agents.Listing;
using Cratis.AI.LanguageModels;
using Cratis.AI.Providers;
using Cratis.AI.Providers.Capacity;
using Cratis.AI.Providers.Pools;
using Cratis.Chronicle.ReadModels;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolPlans;

public class when_the_pool_of_an_agent_cannot_be_read : Specification
{
    sealed class PlansFor(Agent? agent, IReadModels readModels) : PoolPlans(readModels, Substitute.For<IAIProviderCapacities>(), TimeProvider.System)
    {
        protected override Task<Agent?> FindAgent(LanguageModelPurpose purpose) => Task.FromResult(agent);
    }

    static readonly LanguageModelPurpose _purpose = new("prologue-dramaturg");

    Exception _exception;
    PoolPlan? _withoutPool;

    Agent AgentWith(AIProviderPoolId? pool) => new(default!, _purpose, new Cratis.AI.Agents.AgentName("Dramaturg"), default!, default, default, pool, [], default, default);

    async Task Because()
    {
        var readModels = Substitute.For<IReadModels>();
        _exception = await Cratis.Specifications.Catch.Exception(() => new PlansFor(AgentWith(new AIProviderPoolId(Guid.NewGuid())), readModels).For(_purpose, CancellationToken.None));
        _withoutPool = await new PlansFor(AgentWith(null), readModels).For(_purpose, CancellationToken.None);
    }

    [Fact] void should_stop_instead_of_going_around_the_pool() => _exception.ShouldBeOfExactType<PoolNotReady>();
    [Fact] void should_say_the_pool_is_not_ready() => ((PoolNotReady)_exception).Outcome.ShouldEqual(AgentDispatchOutcome.NotReady);
    [Fact] void should_name_the_agent_in_the_message() => _exception.Message.ShouldContain("Dramaturg");
    [Fact] void should_leave_an_agent_with_no_pool_to_cratis_ai() => _withoutPool.ShouldBeNull();
}
#endif
