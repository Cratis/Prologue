// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Discover.Map;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Findings;

/// <summary>
/// Explains decisions with what Discover found in the code: the <see cref="CodeFindings"/> of the prologue, which the analyses
/// keep up to date, seen as decision points. It emits at once, with none, for a prologue nothing was analyzed for.
/// </summary>
/// <param name="readModels">Observes the stored findings.</param>
public class FindingsDecisionReasons(IReadModels readModels) : IDecisionReasons
{
    /// <inheritdoc/>
    public IObservable<IReadOnlyList<DecisionPoint>> Observe(PrologueId prologue) =>
        ApplicationMap.Findings(prologue, readModels).Select(FindingsDecisionPoints.From);
}
