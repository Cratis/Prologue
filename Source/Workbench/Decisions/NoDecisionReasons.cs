// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// The decision reasons while Discover supplies none: no decision points, so every reason is the observed pattern.
/// </summary>
public class NoDecisionReasons : IDecisionReasons
{
    /// <inheritdoc/>
    public IObservable<IReadOnlyList<DecisionPoint>> Observe(PrologueId prologue) =>
        Observable.Return<IReadOnlyList<DecisionPoint>>([]);
}
