// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Abstractions;
using Cratis.AI.Agents;

namespace Cratis.Prologue.Workbench.AI.for_PrologueAIAgents;

public class when_resolving_an_agent_by_purpose : Specification
{
    PrologueAIAgents _agents;
    AgentDescriptor _byPurpose;
    AgentDescriptor _byId;
    AgentDescriptor _unknown;

    void Establish() => _agents = new PrologueAIAgents();

    void Because()
    {
        _byPurpose = _agents.FindByPurpose(PrologueAgents.Usher.Purpose);
        _byId = _agents.Find(AgentId.For(PrologueAgents.Usher.Purpose));
        _unknown = _agents.FindByPurpose(new Cratis.AI.LanguageModels.LanguageModelPurpose("not-a-prologue-agent"));
    }

    [Fact] void should_name_the_agent() => _byPurpose.Name.Value.ShouldEqual("Usher");
    [Fact] void should_resolve_the_same_agent_by_its_id() => _byId.ShouldEqual(_byPurpose);
    [Fact] void should_not_resolve_a_purpose_that_is_not_a_prologue_agent() => _unknown.ShouldBeNull();
    [Fact] void should_describe_the_four_agents() => PrologueAgents.All.Select(_ => _.Name).ShouldContainOnly(["Dramaturg", "Prompter", "Usher", "Interpreter"]);
}
#endif
