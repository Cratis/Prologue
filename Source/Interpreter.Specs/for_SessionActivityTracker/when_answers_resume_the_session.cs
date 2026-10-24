// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_SessionActivityTracker;

public class when_answers_resume_the_session : given.a_tracker
{
    void Establish()
    {
        _tracker.StatusChanged(InterpreterStatus.AwaitingAnswers);
        _tracker.AnswerReceived();
        _tracker.StatusChanged(InterpreterStatus.Refining);
        _clock.Advance(_gracePeriod + _gracePeriod);
    }

    void Because() => _result = Evaluate();

    [Fact] void should_not_count_the_grace_period_while_working() => _result.ShouldEqual(ShutdownReason.None);
}
#endif
