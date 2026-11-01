// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Conversations;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Workbench.AI;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_CratisAIChatClientFactory;

public class when_the_agent_cannot_be_reached : Specification
{
    IAIChatClients _clients;
    IChatClient _agent;
    CratisAIChatClientFactory _factory;
    IChatClient _created;
    Exception _thrown;
    string _failureBeforeTheCall;

    void Establish()
    {
        _clients = Substitute.For<IAIChatClients>();
        _agent = Substitute.For<IChatClient>();
        _agent.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns<Task<ChatResponse>>(_ => throw new InvalidOperationException("No provider is configured for the Interpreter"));
        _clients.For(PrologueAgents.Interpreter.Purpose).Returns(_agent);
        _factory = new CratisAIChatClientFactory(_clients);
    }

    async Task Because()
    {
        _created = _factory.CreateFor(new LlmOptions());
        _failureBeforeTheCall = _factory.Failure;
        _thrown = await Cratis.Specifications.Catch.Exception(() => _created.GetResponseAsync([new ChatMessage(ChatRole.User, "hello")]));
    }

    [Fact] void should_talk_through_the_interpreter_agent() => _clients.Received(1).For(PrologueAgents.Interpreter.Purpose);
    [Fact] void should_not_have_a_failure_before_anything_was_asked() => _failureBeforeTheCall.ShouldBeEmpty();
    [Fact] void should_let_the_failure_through_so_the_session_falls_back() => _thrown.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_say_why_it_failed() => _factory.Failure.ShouldEqual("No provider is configured for the Interpreter");
}
#endif
