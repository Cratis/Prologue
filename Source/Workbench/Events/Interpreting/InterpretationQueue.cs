// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Threading.Channels;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Represents an in-memory <see cref="IInterpretationQueue"/>.
/// </summary>
/// <param name="progress">Told when a job is queued.</param>
public class InterpretationQueue(IInterpretationProgress progress) : IInterpretationQueue
{
    readonly Channel<InterpretationJob> _channel = Channel.CreateUnbounded<InterpretationJob>(new() { SingleReader = true });
    readonly ConcurrentDictionary<PrologueId, byte> _active = new();

    /// <inheritdoc/>
    public bool Enqueue(InterpretationJob job)
    {
        if (!_active.TryAdd(job.Prologue, 0))
        {
            return false;
        }

        progress.Set(new InterpretationProgress(job.Prologue, InterpretationState.Queued, "Waiting for its turn", false, progress.Get(job.Prologue).Suggestions));
        return _channel.Writer.TryWrite(job);
    }

    /// <inheritdoc/>
    public ValueTask<InterpretationJob> Next(CancellationToken cancellationToken) => _channel.Reader.ReadAsync(cancellationToken);

    /// <inheritdoc/>
    public void Done(PrologueId prologue) => _active.TryRemove(prologue, out _);
}
