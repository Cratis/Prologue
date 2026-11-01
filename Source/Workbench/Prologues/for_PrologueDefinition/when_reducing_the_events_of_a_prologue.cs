// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Prologues.Defining;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems;
using Cratis.Prologue.Workbench.Systems.Removing;
using Cratis.Prologue.Workbench.Systems.RuntimeSources;

namespace Cratis.Prologue.Workbench.Prologues.for_PrologueDefinition;

public class when_reducing_the_events_of_a_prologue : a_prologue
{
    PrologueDefinition _result;

    void Because()
    {
        var withClusters = PrologueDefinition.Reduce(new SystemHttpClustersSet(_lending, [new ClusterName("lending-api")]), _prologue);
        _result = PrologueDefinition.Reduce(new SystemRemoved(_search), withClusters);
    }

    [Fact] void should_take_the_identity_from_the_event_source() => _result.Id.ShouldEqual(_id);
    [Fact] void should_start_with_every_evidence_step_on() => _result.Steps.ShouldEqual(EvidenceSteps.All);
    [Fact] void should_keep_the_systems_that_remain() => _result.Systems.Select(system => system.Name.Value).ShouldContainOnly("Lending");
    [Fact] void should_keep_the_clusters_of_the_system() => _result.Systems.Single().HttpClusters.Single().Value.ShouldEqual("lending-api");
}
#endif
