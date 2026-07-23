// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Configuration;
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Screenplay;
using Cratis.Prologue.Storage;

namespace Cratis.Prologue.Interpreter.for_InterpreterSessionHost.given;

public class all_dependencies : Specification
{
    protected static readonly Guid _prologueId = new("11111111-2222-3333-4444-555555555555");

    protected IInterpreterSessionFactory _sessions;
    protected IInterpreterSession _session;
    protected ICaptureStore _captureStore;
    protected IScreenplayGenerator _screenplays;
    protected ISessionActivityTracker _activity;
    protected ILogger _logger;
    protected List<InterpreterSessionState> _persisted;
    protected List<Capture> _captures;
    protected LlmOptions _llmOptions;
    protected InterpreterSessionHost _host;

    void Establish()
    {
        _sessions = Substitute.For<IInterpreterSessionFactory>();
        _session = Substitute.For<IInterpreterSession>();
        _captureStore = Substitute.For<ICaptureStore>();
        _screenplays = Substitute.For<IScreenplayGenerator>();
        _activity = Substitute.For<ISessionActivityTracker>();
        _logger = Substitute.For<ILogger>();
        _persisted = [];
        _llmOptions = new LlmOptions();

        var occurred = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _captures = [new Capture(Guid.NewGuid(), occurred, [], _prologueId)];
        _captureStore.GetForPrologue(_prologueId, Arg.Any<CancellationToken>()).Returns(_captures);

        _sessions
            .CreateNew(Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Capture>>(), Arg.Any<LlmOptions>(), Arg.Any<Action<InterpreterStatus>?>(), Arg.Any<int>())
            .Returns(_session);
        _sessions
            .Resume(Arg.Any<InterpreterSessionState>(), Arg.Any<IReadOnlyList<Capture>>(), Arg.Any<LlmOptions>(), Arg.Any<Action<InterpreterStatus>?>(), Arg.Any<int>())
            .Returns(_session);

        _host = new InterpreterSessionHost(
            _prologueId,
            _sessions,
            _captureStore,
            _screenplays,
            _activity,
            state =>
            {
                _persisted.Add(state);
                return Task.CompletedTask;
            },
            _logger);
    }

    protected void SessionProceedsTo(InterpreterSessionState state)
    {
        // Behave like the real session — the state only reflects the checkpoint once Proceed has run.
        var current = InterpreterSessionState.New(_prologueId);
        _session.State.Returns(_ => current);
        _session.Proceed(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            current = state;
            return state;
        });
    }

    protected static InterpreterSessionState StateWith(InterpreterStatus status, params InterpreterQuestion[] pending) =>
        InterpreterSessionState.New(_prologueId) with { Status = status, PendingQuestions = pending };
}
#endif
