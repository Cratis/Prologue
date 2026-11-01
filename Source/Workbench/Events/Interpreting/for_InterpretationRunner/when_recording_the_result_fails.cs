// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Execution;
using Cratis.Prologue.Workbench.Events.Interpreting.given;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationRunner;

public class when_recording_the_result_fails : an_interpretation_runner
{
    async Task Because()
    {
        _chatClients.Unreachable = "no agent";
        _recordResult = new CommandResult { CorrelationId = CorrelationId.New(), ValidationResults = [ValidationResult.Error("The event store is down")] };
        await Run();
    }

    [Fact] void should_fail() => Progress.State.ShouldEqual(InterpretationState.Failed);
    [Fact] void should_say_why() => Progress.Message.ShouldContain("The event store is down");
}
#endif
