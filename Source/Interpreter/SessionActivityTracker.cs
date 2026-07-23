// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpretation;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents an <see cref="ISessionActivityTracker"/> keeping the timestamps the service's lifecycle decisions
/// are made from — when the last session activity happened, how long a session has been awaiting answers, and
/// whether a finished session's result has been fetched.
/// </summary>
/// <param name="timeProvider">The <see cref="TimeProvider"/> supplying the clock.</param>
public class SessionActivityTracker(TimeProvider timeProvider) : ISessionActivityTracker
{
    readonly Lock _gate = new();
    readonly TimeProvider _timeProvider = timeProvider;
    readonly DateTimeOffset _startedAt = timeProvider.GetUtcNow();
    DateTimeOffset? _lastActivity;
    DateTimeOffset? _awaitingAnswersSince;
    bool _finished;
    bool _resultFetched;

    /// <inheritdoc/>
    public void SessionTouched()
    {
        lock (_gate)
        {
            _lastActivity = _timeProvider.GetUtcNow();
        }
    }

    /// <inheritdoc/>
    public void StatusChanged(InterpreterStatus status)
    {
        lock (_gate)
        {
            var now = _timeProvider.GetUtcNow();
            _lastActivity = now;
            _awaitingAnswersSince = status == InterpreterStatus.AwaitingAnswers ? now : null;
            _finished = status is InterpreterStatus.Completed or InterpreterStatus.Failed;
        }
    }

    /// <inheritdoc/>
    public void AnswerReceived()
    {
        lock (_gate)
        {
            var now = _timeProvider.GetUtcNow();
            _lastActivity = now;
            if (_awaitingAnswersSince is not null)
            {
                _awaitingAnswersSince = now;
            }
        }
    }

    /// <inheritdoc/>
    public void ResultFetched()
    {
        lock (_gate)
        {
            _lastActivity = _timeProvider.GetUtcNow();
            _resultFetched = true;
        }
    }

    /// <inheritdoc/>
    public ShutdownReason Evaluate(TimeSpan gracePeriod, TimeSpan idleTimeout)
    {
        lock (_gate)
        {
            var now = _timeProvider.GetUtcNow();
            if (_awaitingAnswersSince is { } awaitingSince && now - awaitingSince >= gracePeriod)
            {
                return ShutdownReason.GracePeriodExpired;
            }

            if (_finished && _resultFetched && now - (_lastActivity ?? _startedAt) >= idleTimeout)
            {
                return ShutdownReason.IdleAfterCompletion;
            }

            if (_lastActivity is null && now - _startedAt >= idleTimeout)
            {
                return ShutdownReason.NoActivity;
            }

            return ShutdownReason.None;
        }
    }
}
