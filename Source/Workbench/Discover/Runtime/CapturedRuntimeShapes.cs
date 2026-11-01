// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Captures.Live;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Runtime;

/// <summary>
/// Feeds Discover from the captures: the command matrix and the capture aggregates are projected from every recorded capture,
/// so what Discover matches the code against is never counted when a page is opened.
/// </summary>
/// <param name="readModels">Observes the stored matrix and aggregates.</param>
public class CapturedRuntimeShapes(IReadModels readModels) : IRuntimeShapes
{
    /// <inheritdoc/>
    public IObservable<RuntimeEvidence> Observe(PrologueId prologue) =>
        readModels.Observe<CommandMatrix>(matrix => matrix.Id == prologue)
            .CombineLatest(
                readModels.Observe<CaptureAggregates>(aggregates => aggregates.Id == prologue),
                (matrix, aggregates) => CapturedRuntimeEvidence.From(matrix.FirstOrDefault(), aggregates.FirstOrDefault()));
}
