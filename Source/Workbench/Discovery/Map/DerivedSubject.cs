// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Prologue.Workbench.Discovery.Map;

/// <summary>
/// A read-only subject over an observable, for a query whose answer is derived from several sources. Every subscriber gets its own
/// subscription, which ends when it is disposed, so nothing is left running after a client disconnects.
/// </summary>
/// <typeparam name="T">The type of the answer.</typeparam>
/// <param name="source">The derived observable.</param>
public class DerivedSubject<T>(IObservable<T> source) : ISubject<T>
{
    /// <inheritdoc/>
    public void OnCompleted()
    {
    }

    /// <inheritdoc/>
    public void OnError(Exception error)
    {
    }

    /// <inheritdoc/>
    public void OnNext(T value)
    {
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer) => source.Subscribe(observer);
}
