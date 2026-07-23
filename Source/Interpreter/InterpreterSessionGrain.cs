// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Screenplay;
using Cratis.Prologue.Storage;
using Orleans.Concurrency;
using Orleans.Providers;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents the <see cref="IInterpreterSessionGrain"/> — a thin Orleans shell over the
/// <see cref="InterpreterSessionHost"/> that owns the actual decision logic. The grain is reentrant so status
/// polls and answers stay responsive while the session proceeds in the background on the activation's scheduler,
/// and it persists every checkpoint through its MongoDB-backed grain state.
/// </summary>
/// <param name="sessions">The <see cref="IInterpreterSessionFactory"/> creating or resuming the session.</param>
/// <param name="captures">The <see cref="ICaptureStore"/> the session's captures load from.</param>
/// <param name="screenplays">The <see cref="IScreenplayGenerator"/> generating the Screenplay when the session completes.</param>
/// <param name="activity">The <see cref="ISessionActivityTracker"/> lifecycle decisions are reported to.</param>
/// <param name="logger">The logger.</param>
[Reentrant]
[StorageProvider(ProviderName = WellKnownStorageProviders.Default)]
public class InterpreterSessionGrain(
    IInterpreterSessionFactory sessions,
    ICaptureStore captures,
    IScreenplayGenerator screenplays,
    ISessionActivityTracker activity,
    ILogger<InterpreterSessionGrain> logger) : Grain<InterpreterSessionStateDocument>, IInterpreterSessionGrain
{
    InterpreterSessionHost? _host;

    /// <inheritdoc/>
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _host = new InterpreterSessionHost(this.GetPrimaryKey(), sessions, captures, screenplays, activity, Persist, logger);
        if (State.ToState() is { } persisted)
        {
            _host.Restore(persisted);
        }

        return base.OnActivateAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task Start(LlmOptions options) => _host!.Start(options);

    /// <inheritdoc/>
    public Task<SessionSnapshot> GetStatus() => Task.FromResult(_host!.GetSnapshot());

    /// <inheritdoc/>
    public Task Answer(InterpreterAnswer answer) => _host!.Answer(answer);

    /// <inheritdoc/>
    public Task<SessionResult?> GetResult() => Task.FromResult(_host!.GetResult());

    async Task Persist(InterpreterSessionState state)
    {
        State = InterpreterSessionStateDocument.For(this.GetPrimaryKey(), state);
        await WriteStateAsync();
    }
}
