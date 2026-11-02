// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.Interpreting.given;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_InterpretationRunner;

public class when_chronicle_never_answers_the_recording : an_interpretation_runner
{
    async Task Because()
    {
        Recording.ItemTimeout = TimeSpan.FromMilliseconds(100);
        _chatClients.Unreachable = "no agent";
        _pipeline.Execute(Arg.Any<object>(), Arg.Any<ValidationResultSeverity?>()).Returns(_ => new TaskCompletionSource<CommandResult>().Task);
        _runner = Build();
        await Run();
    }

    [Fact] void should_fail() => Progress.State.ShouldEqual(InterpretationState.Failed);
    [Fact] void should_say_chronicle_did_not_answer() => Progress.Message.ShouldContain("Chronicle did not answer");
}
#endif
