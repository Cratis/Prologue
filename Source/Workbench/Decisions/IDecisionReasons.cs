// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// The seam through which what Discover found in the code explains what Capture observed. A decision takes its reason from
/// the decision point matching its command shape and outcome when there is one, and from the observed pattern otherwise.
/// Discover supplies its own implementation, backed by a projection of its decision points (a read model keyed by prologue
/// holding <see cref="DecisionPoint"/> items), and registers it before AddWorkbench so it replaces <see cref="NoDecisionReasons"/>.
/// </summary>
public interface IDecisionReasons
{
    /// <summary>
    /// Observes the decision points found in the code of a prologue's systems.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The decision points, pushed again whenever they change.</returns>
    IObservable<IReadOnlyList<DecisionPoint>> Observe(PrologueId prologue);
}
