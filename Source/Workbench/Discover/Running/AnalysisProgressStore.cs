// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace Cratis.Prologue.Workbench.Discover.Running;

/// <summary>
/// Keeps the progress of analyses in memory.
/// </summary>
public sealed class AnalysisProgressStore : IAnalysisProgress, IDisposable
{
    readonly ConcurrentDictionary<AnalysisSubjectId, AnalysisProgress> _progress = new();
    readonly Lock _gate = new();
    readonly BehaviorSubject<IReadOnlyList<AnalysisProgress>> _subject = new([]);

    /// <inheritdoc/>
    public IObservable<IReadOnlyList<AnalysisProgress>> Observe() => _subject;

    /// <inheritdoc/>
    public void Set(AnalysisProgress progress)
    {
        // A subject is not safe for concurrent OnNext, and two sets racing could also publish an older snapshot last.
        lock (_gate)
        {
            _progress[progress.Target.Subject] = progress;
            _subject.OnNext([.. _progress.Values]);
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _subject.Dispose();

    /// <inheritdoc/>
    public AnalysisProgress? Get(AnalysisSubjectId subject) => _progress.GetValueOrDefault(subject);
}
