// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Conversations;
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Workbench.AI;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_CratisAIChatClientFactory;

public class when_the_agent_answers : Specification
{
    CratisAIChatClientFactory _factory;
    ChatResponse _response;

    void Establish()
    {
        var clients = Substitute.For<IAIChatClients>();
        var agent = Substitute.For<IChatClient>();
        agent.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ChatResponse([new ChatMessage(ChatRole.Assistant, "{}")])));
        clients.For(PrologueAgents.Interpreter.Purpose).Returns(agent);
        _factory = new CratisAIChatClientFactory(clients);
    }

    async Task Because() => _response = await _factory.CreateFor(new LlmOptions()).GetResponseAsync([new ChatMessage(ChatRole.User, "hello")]);

    [Fact] void should_pass_the_answer_on() => _response.Text.ShouldEqual("{}");
    [Fact] void should_report_no_failure() => _factory.Failure.ShouldBeEmpty();
}
#endif
