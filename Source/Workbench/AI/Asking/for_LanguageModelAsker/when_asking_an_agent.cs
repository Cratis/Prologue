// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Conversations;
using Cratis.Prologue.Workbench.AI.Dispatch;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Asking.for_LanguageModelAsker;

public class when_asking_an_agent : Specification
{
    AgentReply _answered;
    AgentReply _paused;
    AgentReply _ceiling;
    AgentReply _down;

    LanguageModelAsker AskerOf(Func<IChatClient> client)
    {
        var clients = Substitute.For<IAIChatClients>();
        clients.For(PrologueAgents.Interpreter.Purpose).Returns(_ => client());
        return new(clients, Microsoft.Extensions.Logging.Abstractions.NullLogger<LanguageModelAsker>.Instance);
    }

    static IChatClient Failing(Exception exception)
    {
        var client = Substitute.For<IChatClient>();
        client.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>()).Returns<Task<ChatResponse>>(_ => throw exception);
        return client;
    }

    async Task Because()
    {
        var answering = Substitute.For<IChatClient>();
        answering.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "an answer"))));
        _answered = await AskerOf(() => answering).Ask(PrologueAgents.Interpreter, "why?", CancellationToken.None);
        _paused = await AskerOf(() => Failing(new AgentDispatchPaused("Code analysis", "Local", "Cloud"))).Ask(PrologueAgents.Interpreter, "why?", CancellationToken.None);
        _ceiling = await AskerOf(() => Failing(new PoolCeilingReached("Code analysis", 1000, 1000))).Ask(PrologueAgents.Interpreter, "why?", CancellationToken.None);
        _down = await AskerOf(() => Failing(new InvalidOperationException("boom"))).Ask(PrologueAgents.Interpreter, "why?", CancellationToken.None);
    }

    [Fact] void should_return_the_answer() => _answered.Text.ShouldEqual("an answer");
    [Fact] void should_say_a_paused_pool_paused() => _paused.Stopped.ShouldEqual(AgentDispatchOutcome.Paused);
    [Fact] void should_carry_the_message_a_person_can_act_on() => _paused.Reason.ShouldContain("Systems page");
    [Fact] void should_say_a_pool_at_its_ceiling_is_at_its_ceiling() => _ceiling.Stopped.ShouldEqual(AgentDispatchOutcome.CeilingReached);
    [Fact] void should_not_fail_the_ask_for_an_unexpected_error() => _down.Succeeded.ShouldBeFalse();
    [Fact] void should_not_call_an_unexpected_error_a_stop() => _down.Stopped.ShouldBeNull();
}
#endif
