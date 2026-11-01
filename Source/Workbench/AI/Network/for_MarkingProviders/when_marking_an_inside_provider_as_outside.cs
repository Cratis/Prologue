// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Providers;
using Cratis.Prologue.Workbench.AI.Network.MarkingProviders;

namespace Cratis.Prologue.Workbench.AI.Network.for_MarkingProviders;

public class when_marking_an_inside_provider_as_outside : Specification
{
    readonly CommandScenario<MarkProviderAsOutsideNetwork> _scenario = new();
    readonly AIProviderId _provider = new(Guid.NewGuid());
    CommandResult _result;

    void Establish() => _scenario.Given.ForEventSource(_provider.Value.ToString()).ReadModel(new ProviderNetworkPosition(_provider.Value.ToString(), true));

    async Task Because() => _result = await _scenario.Execute(new MarkProviderAsOutsideNetwork(_provider));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] async Task should_record_that_it_is_outside() => await _scenario.EventSequence.ShouldHaveAppendedEvent<ProviderMarkedAsOutsideNetwork>();
}
#endif
