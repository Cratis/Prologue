// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.AI.Conversations;
using Cratis.AI.Providers;
using Cratis.Prologue.Workbench.AI.Pools;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch.given;

public static class pools
{
    public static readonly ChatAgentTarget Agent = new("Dramaturg", "Maps code", ModelTier.Balanced, null, null);

    public static IEnumerable<ChatMessage> Hello => [new ChatMessage(ChatRole.User, "hello")];

    public static DispatchMember Inside(string name, double headroom = 1, bool canStart = true) => new(new AIProviderId(Guid.NewGuid()), name, true, headroom, canStart, canStart ? string.Empty : "has no capacity left");

    public static DispatchMember Outside(string name, double headroom = 1, bool canStart = true) => new(new AIProviderId(Guid.NewGuid()), name, false, headroom, canStart, canStart ? string.Empty : "has no capacity left");

    public static PoolPlan Plan(PoolFallback fallback, long ceiling, long used, params DispatchMember[] members) => new(Guid.NewGuid().ToString(), "Code analysis", fallback, members, Agent, ceiling, used);

    public static IChatClient Answering(long tokens, int delayMilliseconds = 0)
    {
        var client = Substitute.For<IChatClient>();
        client.GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>()).Returns(async _ =>
        {
            await Task.Delay(delayMilliseconds);
            return new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")) { Usage = new UsageDetails { TotalTokenCount = tokens } };
        });
        return client;
    }

    public static Func<DispatchMember, Task<string>> Attempts(List<string> tried, params string[] failing) => member =>
    {
        tried.Add(member.Name);
        return failing.Contains(member.Name) ? throw new InvalidOperationException($"{member.Name} is down") : Task.FromResult(member.Name);
    };
}
#endif
