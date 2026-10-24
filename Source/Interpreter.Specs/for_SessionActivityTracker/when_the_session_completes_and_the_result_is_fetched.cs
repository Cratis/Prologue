// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_SessionActivityTracker;

public class when_the_session_completes_and_the_result_is_fetched : given.a_tracker
{
    void Establish()
    {
        _tracker.StatusChanged(InterpreterStatus.Completed);
        _tracker.ResultFetched();
        _clock.Advance(_idleTimeout);
    }

    void Because() => _result = Evaluate();

    [Fact] void should_exit_for_idling_after_completion() => _result.ShouldEqual(ShutdownReason.IdleAfterCompletion);
}
#endif
