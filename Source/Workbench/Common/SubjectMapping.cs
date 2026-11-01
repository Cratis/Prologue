// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;

namespace Cratis.Prologue.Workbench.Common;

/// <summary>
/// Maps what an observable query pushes into another shape.
/// </summary>
public static class SubjectMapping
{
    /// <summary>
    /// Maps every value a subject pushes. The result completes when the source does.
    /// </summary>
    /// <typeparam name="TSource">The type the source pushes.</typeparam>
    /// <typeparam name="TResult">The type to push instead.</typeparam>
    /// <param name="source">The source subject.</param>
    /// <param name="map">Maps one value.</param>
    /// <returns>A subject pushing the mapped values, replaying the latest to a late subscriber.</returns>
    public static ISubject<TResult> Mapped<TSource, TResult>(this ISubject<TSource> source, Func<TSource, TResult> map)
    {
        var mapped = new ReplaySubject<TResult>(1);
        source.Select(map).Subscribe(mapped);
        return mapped;
    }
}
