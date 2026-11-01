// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Providers;
using Cratis.Prologue.Workbench.AI.Network.MarkingProviders;

namespace Cratis.Prologue.Workbench.AI.Network.for_MarkingProviders;

public class when_marking_a_provider_that_was_never_marked : Specification
{
    readonly CommandScenario<MarkProviderAsInsideNetwork> _scenario = new();
    CommandResult _result;

    async Task Because() => _result = await _scenario.Execute(new MarkProviderAsInsideNetwork(new AIProviderId(Guid.NewGuid())));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] async Task should_record_that_it_is_inside() => await _scenario.EventSequence.ShouldHaveAppendedEvent<ProviderMarkedAsInsideNetwork>();
}
#endif
