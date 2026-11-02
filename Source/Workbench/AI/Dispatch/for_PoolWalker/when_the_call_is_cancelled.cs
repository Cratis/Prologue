// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.AI.Dispatch.given;
using Cratis.Prologue.Workbench.AI.Pools;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolWalker;

public class when_the_call_is_cancelled : Specification
{
    Exception _exception;
    string _afterATimeout;
    List<string> _tried = [];

    async Task Because()
    {
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        var plan = pools.Plan(PoolFallback.ContinueOnNext, 0, 0, pools.Inside("Local"), pools.Outside("Cloud"));
        Task<string> cancelling(DispatchMember member)
        {
            _tried.Add(member.Name);
            throw new OperationCanceledException(cancelled.Token);
        }

        _exception = await Cratis.Specifications.Catch.Exception(() => PoolWalker.Walk(plan, cancelling, cancelled.Token));

        // A vendor's HttpClient timeout is a TaskCanceledException while the caller's token is still live: the member failed, the caller did not cancel.
        _afterATimeout = await PoolWalker.Walk(pools.Plan(PoolFallback.ContinueOnNext, 0, 0, pools.Inside("Local"), pools.Outside("Cloud")), member => member.Name == "Local" ? throw new TaskCanceledException("timed out") : Task.FromResult(member.Name), CancellationToken.None);
    }

    [Fact] void should_not_move_on_to_the_next_member() => _exception.ShouldBeOfExactType<OperationCanceledException>();
    [Fact] void should_stop_after_the_first_member() => _tried.ShouldContainOnly("Local");
    [Fact] void should_treat_a_timeout_as_a_member_failure() => _afterATimeout.ShouldEqual("Cloud");
}
#endif
