// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Represents an in-memory <see cref="IInterpretationProgress"/>.
/// </summary>
public sealed class InterpretationProgressStore : IInterpretationProgress, IDisposable
{
    readonly ConcurrentDictionary<PrologueId, BehaviorSubject<InterpretationProgress>> _subjects = new();

    /// <inheritdoc/>
    public IObservable<InterpretationProgress> Observe(PrologueId prologue) => SubjectOf(prologue);

    /// <inheritdoc/>
    public InterpretationProgress Get(PrologueId prologue) => SubjectOf(prologue).Value;

    /// <inheritdoc/>
    public void Set(InterpretationProgress progress)
    {
        // A subject is not safe for concurrent OnNext.
        var subject = SubjectOf(progress.Prologue);
        lock (subject)
        {
            subject.OnNext(progress);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var subject in _subjects.Values)
        {
            subject.Dispose();
        }
    }

    BehaviorSubject<InterpretationProgress> SubjectOf(PrologueId prologue) =>
        _subjects.GetOrAdd(prologue, id => new BehaviorSubject<InterpretationProgress>(InterpretationProgress.Idle(id)));
}
