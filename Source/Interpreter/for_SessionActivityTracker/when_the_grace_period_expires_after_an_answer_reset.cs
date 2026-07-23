// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_SessionActivityTracker;

public class when_the_grace_period_expires_after_an_answer_reset : given.a_tracker
{
    void Establish()
    {
        _tracker.StatusChanged(InterpreterStatus.AwaitingAnswers);
        _clock.Advance(_gracePeriod - TimeSpan.FromSeconds(1));
        _tracker.AnswerReceived();
        _clock.Advance(_gracePeriod);
    }

    void Because() => _result = Evaluate();

    [Fact] void should_exit_for_the_expired_grace_period() => _result.ShouldEqual(ShutdownReason.GracePeriodExpired);
}
#endif
