// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Interpreter.for_SessionActivityTracker.given;

public class a_tracker : Specification
{
    protected static readonly TimeSpan _gracePeriod = TimeSpan.FromSeconds(300);
    protected static readonly TimeSpan _idleTimeout = TimeSpan.FromSeconds(600);

    protected test_clock _clock;
    protected SessionActivityTracker _tracker;
    protected ShutdownReason _result;

    void Establish()
    {
        _clock = new test_clock();
        _tracker = new SessionActivityTracker(_clock);
    }

    protected ShutdownReason Evaluate() => _tracker.Evaluate(_gracePeriod, _idleTimeout);
}
#endif
