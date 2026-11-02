// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Conversations;
using Cratis.AI.LanguageModels;
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_DispatchingChatClient;

public class when_the_agent_has_no_pool : Specification
{
    IPoolSpendRecorder _spend;
    ChatResponse _response;

    async Task Because()
    {
        var purpose = new LanguageModelPurpose("prologue-dramaturg");
        var direct = Substitute.For<IChatClient>();
        direct.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "directly"))));
        var inner = Substitute.For<IAIChatClients>();
        inner.For(purpose).Returns(direct);
        var plans = Substitute.For<IPoolPlans>();
        plans.For(purpose, Arg.Any<CancellationToken>()).Returns(Task.FromResult<PoolPlan?>(null));
        _spend = Substitute.For<IPoolSpendRecorder>();
        _response = await new DispatchingChatClient(purpose, inner, plans, _spend).GetResponseAsync(pools.Hello, null, TestContext.Current.CancellationToken);
    }

    [Fact] void should_leave_the_call_to_cratis_ai() => _response.Text.ShouldEqual("directly");
    [Fact] void should_record_no_spend() => _spend.DidNotReceiveWithAnyArgs().Record(default!, default, TestContext.Current.CancellationToken);
}
#endif
