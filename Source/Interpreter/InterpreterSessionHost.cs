// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Screenplay;
using Cratis.Prologue.Storage;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Hosts one Prologue's interpreter session for the service mode — the decision logic the session grain delegates
/// to, kept free of Orleans so it is testable on its own. Starting is idempotent: the first start creates the
/// session from the stored captures (resuming from a previously persisted state when one exists) and kicks the
/// session's <c>Proceed</c> off in the background; later starts just continue it. Every checkpoint the session
/// reaches is handed to the persist callback, so a process exit at any time loses nothing.
/// </summary>
/// <param name="prologueId">The Prologue the hosted session interprets captures for.</param>
/// <param name="sessions">The <see cref="IInterpreterSessionFactory"/> creating or resuming the session.</param>
/// <param name="captures">The <see cref="ICaptureStore"/> the session's captures load from.</param>
/// <param name="screenplays">The <see cref="IScreenplayGenerator"/> generating the Screenplay when the session completes.</param>
/// <param name="activity">The <see cref="ISessionActivityTracker"/> lifecycle decisions are reported to.</param>
/// <param name="persist">The callback invoked with every checkpointed <see cref="InterpreterSessionState"/>.</param>
/// <param name="logger">The logger.</param>
public class InterpreterSessionHost(
    Guid prologueId,
    IInterpreterSessionFactory sessions,
    ICaptureStore captures,
    IScreenplayGenerator screenplays,
    ISessionActivityTracker activity,
    Func<InterpreterSessionState, Task> persist,
    ILogger logger)
{
    IInterpreterSession? _session;
    InterpreterSessionState? _restored;
    string _screenplay = string.Empty;

    /// <summary>
    /// Gets the in-flight background work — the session proceeding towards its next checkpoint. Completed when
    /// the session is parked or finished.
    /// </summary>
    public Task Work { get; private set; } = Task.CompletedTask;

    /// <summary>
    /// Hands the host a previously persisted state to resume from when the session starts.
    /// </summary>
    /// <param name="state">The <see cref="InterpreterSessionState"/> to resume from.</param>
    public void Restore(InterpreterSessionState state) => _restored = state;

    /// <summary>
    /// Starts the session, or continues it when it is already running — idempotent. The first start loads the
    /// Prologue's captures and creates the session, resuming from the restored state when one exists.
    /// </summary>
    /// <param name="llmOptions">The language-model configuration for the session.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Start(LlmOptions llmOptions)
    {
        activity.SessionTouched();
        if (_session is null)
        {
            var prologueCaptures = await captures.GetForPrologue(prologueId);
            _session = _restored is not null
                ? sessions.Resume(_restored, prologueCaptures, llmOptions, activity.StatusChanged)
                : sessions.CreateNew(prologueId, prologueCaptures, llmOptions, activity.StatusChanged);
        }

        BeginWork();
    }

    /// <summary>
    /// Gets the snapshot of the session as it stands — the restored state before the session starts, and a
    /// not-started state when nothing exists yet.
    /// </summary>
    /// <returns>The <see cref="SessionSnapshot"/>.</returns>
    public SessionSnapshot GetSnapshot() => SessionSnapshot.From(CurrentState());

    /// <summary>
    /// Records the answer for one pending question and continues the session in the background when every pending
    /// question has been answered.
    /// </summary>
    /// <param name="answer">The <see cref="InterpreterAnswer"/> to record.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="QuestionNotPending">Thrown when the answer refers to a question that is not pending.</exception>
    public async Task Answer(InterpreterAnswer answer)
    {
        var session = _session ?? throw new QuestionNotPending(answer.QuestionId);
        var state = session.Answer(answer);
        activity.AnswerReceived();
        await persist(state);
        if (state.PendingQuestions.Count == 0)
        {
            BeginWork();
        }
    }

    /// <summary>
    /// Gets the session's result once it has completed.
    /// </summary>
    /// <returns>The <see cref="SessionResult"/>, or <see langword="null"/> until the session has completed.</returns>
    public SessionResult? GetResult()
    {
        var state = CurrentState();
        if (state.Status != InterpreterStatus.Completed || state.Model is null)
        {
            return null;
        }

        // A restart after completion loses the in-memory Screenplay — regenerate it from the persisted model.
        if (_screenplay.Length == 0)
        {
            _screenplay = screenplays.Generate(state.Model);
        }

        activity.ResultFetched();
        return new SessionResult(state.Model, _screenplay);
    }

    InterpreterSessionState CurrentState() => _session?.State ?? _restored ?? InterpreterSessionState.New(prologueId);

    void BeginWork()
    {
        if (!Work.IsCompleted || CurrentState().Status is InterpreterStatus.Completed or InterpreterStatus.Failed)
        {
            return;
        }

        Work = ProceedAndPersist();
    }

    async Task ProceedAndPersist()
    {
        try
        {
            var state = await _session!.Proceed();
            if (state is { Status: InterpreterStatus.Completed, Model: not null })
            {
                _screenplay = screenplays.Generate(state.Model);
            }

            await persist(state);
        }
        catch (Exception exception)
        {
            InterpreterSessionHostLog.WorkFailed(logger, exception);
        }
    }
}
