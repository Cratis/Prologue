// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Interpreting.given;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationRunner;

public class when_something_unexpected_goes_wrong : an_interpretation_runner
{
    Exception _thrown;

    async Task Because()
    {
        _evidence.Load(Arg.Any<PrologueId>(), Arg.Any<CancellationToken>()).Returns<Task<InterpretationInput>>(_ => throw new InvalidOperationException("the capture store is down"));
        _thrown = await Cratis.Specifications.Catch.Exception(() => Run());
    }

    [Fact] void should_not_throw_so_the_worker_keeps_going() => _thrown.ShouldBeNull();
    [Fact] void should_report_the_failure() => Progress.State.ShouldEqual(InterpretationState.Failed);
}
#endif
