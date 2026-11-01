// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Cratis.Prologue.Workbench.Discover.Running;

/// <summary>
/// Queues analyses in memory and shows each as waiting in the progress as soon as it is queued.
/// </summary>
/// <param name="progress">Holds the progress shown while a job waits.</param>
public class AnalysisQueue(IAnalysisProgress progress) : IAnalysisQueue
{
    readonly Channel<AnalysisJob> _channel = Channel.CreateUnbounded<AnalysisJob>(new() { SingleReader = true });
    readonly ConcurrentDictionary<AnalysisSubjectId, byte> _active = new();

    /// <inheritdoc/>
    public bool Enqueue(AnalysisJob job)
    {
        if (!_active.TryAdd(job.Target.Subject, 0))
        {
            return false;
        }

        progress.Set(AnalysisProgress.Queued(job));
        return _channel.Writer.TryWrite(job);
    }

    /// <inheritdoc/>
    public ValueTask<AnalysisJob> Next(CancellationToken cancellationToken) => _channel.Reader.ReadAsync(cancellationToken);

    /// <inheritdoc/>
    public void Done(AnalysisJob job) => _active.TryRemove(job.Target.Subject, out _);
}
