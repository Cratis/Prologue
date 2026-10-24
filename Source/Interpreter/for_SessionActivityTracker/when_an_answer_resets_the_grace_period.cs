// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_SessionActivityTracker;

public class when_an_answer_resets_the_grace_period : given.a_tracker
{
    void Establish()
    {
        _tracker.StatusChanged(InterpreterStatus.AwaitingAnswers);
        _clock.Advance(_gracePeriod - TimeSpan.FromSeconds(1));
        _tracker.AnswerReceived();
        _clock.Advance(_gracePeriod - TimeSpan.FromSeconds(1));
    }

    void Because() => _result = Evaluate();

    [Fact] void should_keep_running() => _result.ShouldEqual(ShutdownReason.None);
}
#endif
