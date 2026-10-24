// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_SessionActivityTracker;

public class when_no_session_activity_happens_within_the_idle_timeout : given.a_tracker
{
    void Establish() => _clock.Advance(_idleTimeout);

    void Because() => _result = Evaluate();

    [Fact] void should_exit_for_no_activity() => _result.ShouldEqual(ShutdownReason.NoActivity);
}
#endif
