// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_SessionActivityTracker;

public class when_evaluating_before_the_idle_timeout : given.a_tracker
{
    void Establish() => _clock.Advance(_idleTimeout - TimeSpan.FromSeconds(1));

    void Because() => _result = Evaluate();

    [Fact] void should_keep_running() => _result.ShouldEqual(ShutdownReason.None);
}
#endif
