// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter.for_SessionActivityTracker;

public class when_the_session_completes_without_the_result_being_fetched : given.a_tracker
{
    void Establish()
    {
        _tracker.StatusChanged(InterpreterStatus.Completed);
        _clock.Advance(_idleTimeout + _idleTimeout);
    }

    void Because() => _result = Evaluate();

    [Fact] void should_wait_for_the_result_to_be_fetched() => _result.ShouldEqual(ShutdownReason.None);
}
#endif
